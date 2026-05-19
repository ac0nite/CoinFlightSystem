using UnityEngine;

namespace CoinFlight
{
    /// <summary>
    /// Кэшированный снимок состояния flight-канваса. Используется
    /// <see cref="CoordinateConverters"/> для конвертации source/target endpoint-ов
    /// в canvas-local space на момент <c>Play()</c>.
    /// Триггеры инвалидации (план §14.7):
    ///   - изменение разрешения, scale factor, render mode, worldCamera,
    ///     safe area / ориентации устройства.
    /// Стратегия: <see cref="Version"/> инкрементируется при любой инвалидации;
    /// предвычисленные снимки хранят version и считаются устаревшими, если он
    /// больше не совпадает с текущей версией контекста.
    /// <para>
    /// Частота проверки: один раз за service tick, НЕ per-coin и НЕ per-frame.
    /// </para>
    /// </summary>
    public sealed class CoordinateContext
    {
        public Canvas Canvas { get; private set; }
        public RectTransform CanvasRect { get; private set; }
        public RenderMode RenderMode { get; private set; }
        public Camera WorldCamera { get; private set; }
        public float ScaleFactor { get; private set; }
        public int Version { get; private set; }

        private int _signature;
        private bool _initialized;

        public CoordinateContext(Canvas canvas)
        {
            if (canvas == null)
                throw new System.ArgumentNullException(nameof(canvas));
            Canvas = canvas;
            CanvasRect = (RectTransform)canvas.transform;
            Version = 1;
        }

        /// <summary>
        /// Перечитывает состояние канваса и инкрементирует <see cref="Version"/>,
        /// если что-то значимое изменилось. Возвращает <c>true</c>, если контекст
        /// был инвалидирован этим вызовом.
        /// </summary>
        public bool RefreshIfDirty()
        {
            int sig = ComputeSignature();
            if (_initialized && sig == _signature)
                return false;

            _signature = sig;
            _initialized = true;
            SnapshotCanvasState();
            Version++;
            return true;
        }

        /// <summary>
        /// Возвращает значение камеры, которое нужно передавать в
        /// <c>RectTransformUtility</c> для этого канваса: <c>null</c> для
        /// <see cref="RenderMode.ScreenSpaceOverlay"/>, иначе
        /// <see cref="Canvas.worldCamera"/>.
        /// </summary>
        public Camera GetRaycastCamera()
        {
            return RenderMode == RenderMode.ScreenSpaceOverlay ? null : WorldCamera;
        }

        private void SnapshotCanvasState()
        {
            RenderMode = Canvas.renderMode;
            WorldCamera = Canvas.worldCamera;
            ScaleFactor = Canvas.scaleFactor;
        }

        private int ComputeSignature()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Screen.width;
                hash = hash * 31 + Screen.height;
                hash = hash * 31 + Mathf.RoundToInt(Canvas.scaleFactor * 1024f);
                hash = hash * 31 + (int)Canvas.renderMode;
                var cam = Canvas.worldCamera;
                hash = hash * 31 + (cam == null ? 0 : cam.GetInstanceID());
                var sa = Screen.safeArea;
                hash = hash * 31 + Mathf.RoundToInt(sa.x);
                hash = hash * 31 + Mathf.RoundToInt(sa.y);
                hash = hash * 31 + Mathf.RoundToInt(sa.width);
                hash = hash * 31 + Mathf.RoundToInt(sa.height);
                return hash;
            }
        }
    }
}
