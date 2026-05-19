namespace CoinFlight
{
    /// <summary>
    /// Группа полёта, обеспеченная одним <c>Tween.Custom</c> на монету. Владеет
    /// массивом per-coin <see cref="CoinTickContext"/>, координирует <see cref="Stop"/>
    /// с пулом / реестром и считает завершения монет, чтобы самоудалиться
    /// из реестра по окончании всех tween-ов.
    /// </summary>
    public sealed class PrimeTweenCoinGroup : ICoinFlightGroup
    {
        private readonly CoinTickContext[] _contexts;
        private readonly int _coinCount;
        private readonly UICoinPool _pool;
        private readonly HandleRegistry _registry;
        private readonly ICoinRenderer _renderer;

        private CoinFlightHandle _handle;
        private int _remaining;
        private bool _alive;

        public bool IsAlive => _alive;
        public int CoinCount => _coinCount;
        internal ICoinRenderer Renderer => _renderer;

        internal PrimeTweenCoinGroup(
            CoinTickContext[] contexts,
            int coinCount,
            ICoinRenderer renderer,
            UICoinPool pool,
            HandleRegistry registry)
        {
            _contexts = contexts;
            _coinCount = coinCount;
            _renderer = renderer;
            _pool = pool;
            _registry = registry;
            _remaining = coinCount;
            _alive = true;
        }

        internal void SetHandle(CoinFlightHandle handle) => _handle = handle;

        internal void Tick(CoinTickContext ctx, float t)
        {
            var state = MotionPatternEvaluator.Evaluate(in ctx.Data, t);
            _renderer.Apply(ctx.View, state);
        }

        internal void OnCoinCompleted(CoinTickContext ctx)
        {
            if (!_alive) return;
            _pool.Release(ctx.PoolSlot);

            _remaining--;
            if (_remaining <= 0)
                Finish();
        }

        public void Stop(CancelPolicy policy)
        {
            if (!_alive) return;
            _alive = false;

            for (int i = 0; i < _coinCount; i++)
            {
                var ctx = _contexts[i];
                if (ctx == null) continue;

                if (ctx.Tween.isAlive)
                {
                    if (policy == CancelPolicy.SnapToTarget)
                        ctx.Tween.Complete();
                    else
                        ctx.Tween.Stop();
                }

                // Guarded by UICoinPool: no-op if already released.
                _pool.Release(ctx.PoolSlot);
            }

            _registry.Unregister(_handle);
        }

        private void Finish()
        {
            _alive = false;
            _registry.Unregister(_handle);
        }
    }
}
