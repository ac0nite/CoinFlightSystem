using Unity.Mathematics;

namespace CoinFlight
{
    /// <summary>
    /// Единственный контракт между слоями Simulation и Rendering. Evaluator
    /// возвращает это значение, renderer его применяет. Canvas local space
    /// (пиксели) — конвертация делается один раз на монету на <c>Play()</c>.
    /// </summary>
    public readonly struct CoinVisualState
    {
        public readonly float2 Position;
        public readonly float RotationDeg;
        public readonly float Scale;
        public readonly float Alpha;
        public readonly CoinVisualFlags Flags;

        public CoinVisualState(float2 position, float rotationDeg, float scale, float alpha, CoinVisualFlags flags)
        {
            Position = position;
            RotationDeg = rotationDeg;
            Scale = scale;
            Alpha = alpha;
            Flags = flags;
        }
    }
}
