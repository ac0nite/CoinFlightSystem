using System;
using Unity.Mathematics;
using UnityEngine;

namespace CoinFlight
{
    /// <summary>
    /// Точка входа в Coin Flight System. Координирует:
    ///   - пул <see cref="UICoinView"/>;
    ///   - конвертацию endpoint -> canvas-local через <see cref="CoordinateContext"/>;
    ///   - предвычисление <see cref="CoinMotionData"/> через <see cref="MotionDataBuilder"/>;
    ///   - бекенд симуляции (<see cref="ICoinSimulationBackend"/>, по умолчанию <see cref="PrimeTweenSimulation"/>);
    ///   - рендерер (<see cref="ICoinRenderer"/>, по умолчанию <see cref="RectTransformRenderer"/>);
    ///   - реестр хэндлов и lifecycle-хуки.
    /// Сервис не владеет <see cref="GameObject"/> flight-канваса — его передаёт вызывающий код.
    /// </summary>
    public sealed class CoinFlightService : IDisposable
    {
        private readonly CoinFlightConfig _config;
        private readonly Canvas _canvas;
        private readonly UICoinPool _pool;
        private readonly HandleRegistry _registry;
        private readonly LifecycleHub _lifecycle;
        private readonly CoordinateContext _context;
        private readonly ICoinRenderer _renderer;
        private readonly ICoinSimulationBackend _backend;

        private CoinMotionData[] _dataBuf;
        private UICoinView[] _viewBuf;
        private int[] _slotBuf;

        private bool _disposed;

        public Canvas FlightCanvas => _canvas;
        public UICoinPool Pool => _pool;
        public int ActiveGroupCount => _registry.ActiveCount;

        public CoinFlightService(
            CoinFlightConfig config,
            Canvas flightCanvas,
            UICoinView coinPrefab,
            ICoinRenderer renderer = null,
            ICoinSimulationBackend backend = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (flightCanvas == null) throw new ArgumentNullException(nameof(flightCanvas));
            if (coinPrefab == null) throw new ArgumentNullException(nameof(coinPrefab));

            _config = config;
            _canvas = flightCanvas;
            _canvas.sortingOrder = config.CanvasSortingOrder;

            var canvasRect = (RectTransform)_canvas.transform;

            _pool = new UICoinPool(coinPrefab, canvasRect, config.PoolPrewarm, config.PoolMax, config.OverflowPolicy);
            _registry = new HandleRegistry(config.MaxConcurrentTweens);
            _context = new CoordinateContext(_canvas);
            _renderer = renderer ?? new RectTransformRenderer();
            _backend = backend ?? new PrimeTweenSimulation();
            _backend.Prewarm(config.MaxConcurrentTweens);

            int initialBuf = Mathf.Max(16, config.PoolMax);
            _dataBuf = new CoinMotionData[initialBuf];
            _viewBuf = new UICoinView[initialBuf];
            _slotBuf = new int[initialBuf];

            _lifecycle = new LifecycleHub(StopAll);
        }

        public CoinFlightHandle Play(in CoinFlightRequest request)
        {
            if (_disposed) return CoinFlightHandle.Invalid;
            if (request.Count <= 0) return CoinFlightHandle.Invalid;
            if (request.Duration < 0f) return CoinFlightHandle.Invalid;

            _context.RefreshIfDirty();

            float2 start = CoordinateConverters.ToCanvasLocal(in request.Source, _context);
            float2 end = CoordinateConverters.ToCanvasLocal(in request.Target, _context);

            int count = request.Count;
            EnsureBuffers(count);

            MotionDataBuilder.Build(in request, start, end, _config, _dataBuf, count);

            int acquired = 0;
            for (int i = 0; i < count; i++)
            {
                if (!_pool.TryAcquire(out int slot, out var view)) break;
                view.Sprite = ResolveSprite(in request, i);
                _slotBuf[i] = slot;
                _viewBuf[i] = view;
                acquired++;
            }

            if (acquired == 0)
                return CoinFlightHandle.Invalid;

            var handle = _registry.ReserveSlot();
            if (!handle.IsValid)
            {
                // Rollback acquired pool slots.
                for (int i = 0; i < acquired; i++)
                    _pool.Release(_slotBuf[i]);
                return CoinFlightHandle.Invalid;
            }

            var group = _backend.StartGroup(
                handle,
                in request,
                _dataBuf,
                _viewBuf,
                _slotBuf,
                acquired,
                _renderer,
                _pool,
                _registry);

            if (group == null)
            {
                _registry.ReleaseReservation(handle);
                for (int i = 0; i < acquired; i++)
                    _pool.Release(_slotBuf[i]);
                return CoinFlightHandle.Invalid;
            }

            _registry.AttachGroup(handle, group);
            return handle;
        }

        public void Stop(CoinFlightHandle handle, CancelPolicy policy)
        {
            if (_disposed) return;
            if (_registry.TryGet(handle, out var group))
                group.Stop(policy);
        }

        public void StopAll(CancelPolicy policy)
        {
            if (_disposed) return;
            _registry.StopAll(policy);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _registry.StopAll(CancelPolicy.InstantHide);
            _lifecycle.Dispose();
        }

        private void EnsureBuffers(int count)
        {
            if (_dataBuf.Length < count) Array.Resize(ref _dataBuf, count);
            if (_viewBuf.Length < count) Array.Resize(ref _viewBuf, count);
            if (_slotBuf.Length < count) Array.Resize(ref _slotBuf, count);
        }

        private static Sprite ResolveSprite(in CoinFlightRequest request, int coinIndex)
        {
            var randomSprites = request.RandomSprites;
            if (randomSprites != null && randomSprites.Length > 0)
            {
                uint hash = WangHash.Derive(request.RandomSeed, (uint)coinIndex);
                return randomSprites[hash % randomSprites.Length];
            }

            return request.Sprite;
        }
    }
}
