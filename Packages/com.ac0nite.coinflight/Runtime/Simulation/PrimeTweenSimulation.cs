using System;
using PrimeTween;

namespace CoinFlight
{
    /// <summary>
    /// Бекенд симуляции Phase 1. Управляет каждой монетой через один non-capturing
    /// <see cref="Tween.Custom{T}(T, float, float, float, System.Action{T,float}, Ease, int, CycleMode, float, float, bool, bool)"/>
    /// с <see cref="Ease.Linear"/>; сам easing делается внутри
    /// <see cref="MotionPatternEvaluator"/> (план §7).
    /// Zero-alloc после <see cref="Prewarm"/> (сама группа / контексты аллоцируются
    /// один раз на <c>Play()</c> — приемлемо в Phase 1).
    /// </summary>
    public sealed class PrimeTweenSimulation : ICoinSimulationBackend
    {
        private static readonly Action<CoinTickContext, float> TickLambda =
            static (ctx, t) => ctx.Group.Tick(ctx, t);

        private static readonly Action<CoinTickContext> CompleteLambda =
            static ctx => ctx.Group.OnCoinCompleted(ctx);

        public void Prewarm(int tweenCapacity)
        {
            if (tweenCapacity < 1) tweenCapacity = 1;
            PrimeTweenConfig.SetTweensCapacity(tweenCapacity);
        }

        public ICoinFlightGroup StartGroup(
            CoinFlightHandle handle,
            in CoinFlightRequest request,
            CoinMotionData[] motionData,
            UICoinView[] views,
            int[] poolSlots,
            int coinCount,
            ICoinRenderer renderer,
            UICoinPool pool,
            HandleRegistry registry)
        {
            var contexts = new CoinTickContext[coinCount];
            var group = new PrimeTweenCoinGroup(contexts, coinCount, renderer, pool, registry);
            group.SetHandle(handle);

            bool unscaled = request.TimeMode == TimeMode.Unscaled;

            for (int i = 0; i < coinCount; i++)
            {
                var ctx = new CoinTickContext
                {
                    Group = group,
                    Data = motionData[i],
                    View = views[i],
                    PoolSlot = poolSlots[i],
                    CoinIndex = i
                };
                contexts[i] = ctx;

                float startDelay = request.StartDelay + request.StaggerPerCoin * i;

                var tween = Tween.Custom(
                    target: ctx,
                    startValue: 0f,
                    endValue: 1f,
                    duration: request.Duration,
                    onValueChange: TickLambda,
                    ease: Ease.Linear,
                    startDelay: startDelay,
                    useUnscaledTime: unscaled);

                tween = tween.OnComplete(target: ctx, onComplete: CompleteLambda);

                ctx.Tween = tween;
            }

            return group;
        }
    }
}
