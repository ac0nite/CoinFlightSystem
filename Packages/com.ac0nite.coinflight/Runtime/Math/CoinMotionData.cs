using Unity.Mathematics;

namespace CoinFlight
{
    /// <summary>
    /// Предвычисленные per-coin данные движения. Полностью самодостаточны,
    /// чтобы evaluator оставался pure, stateless и Burst-совместимым.
    /// Вычисляются один раз на <c>Play()</c> и больше не мутируются.
    /// </summary>
    public readonly struct CoinMotionData
    {
        public readonly float2 StartPos;
        public readonly float2 EndPos;

        public readonly float2 ControlPoint;
        public readonly float2 ScatterPoint;

        public readonly float ArcHeight;
        public readonly float2 ArcAxis;

        public readonly float ScatterEnd;

        public readonly MotionPattern Pattern;
        public readonly EasingType Easing;

        public readonly uint Seed;

        public CoinMotionData(
            float2 startPos,
            float2 endPos,
            float2 controlPoint,
            float2 scatterPoint,
            float arcHeight,
            float2 arcAxis,
            float scatterEnd,
            MotionPattern pattern,
            EasingType easing,
            uint seed)
        {
            StartPos = startPos;
            EndPos = endPos;
            ControlPoint = controlPoint;
            ScatterPoint = scatterPoint;
            ArcHeight = arcHeight;
            ArcAxis = arcAxis;
            ScatterEnd = scatterEnd;
            Pattern = pattern;
            Easing = easing;
            Seed = seed;
        }
    }
}
