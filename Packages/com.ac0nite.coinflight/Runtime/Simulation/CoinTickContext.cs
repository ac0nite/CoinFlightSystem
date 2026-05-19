using PrimeTween;

namespace CoinFlight
{
    /// <summary>
    /// Per-coin контекст анимации, передаваемый как <c>target</c> в non-capturing
    /// <see cref="Tween.Custom"/>. Позволяет использовать zero-alloc статические
    /// лямбды: <c>static (ctx, t) => ctx.Group.Tick(ctx, t)</c>.
    /// Reference type (class) — PrimeTween требует ссылочный target.
    /// </summary>
    internal sealed class CoinTickContext
    {
        public PrimeTweenCoinGroup Group;
        public CoinMotionData Data;
        public UICoinView View;
        public int CoinIndex;
        public int PoolSlot;
        public Tween Tween;
    }
}
