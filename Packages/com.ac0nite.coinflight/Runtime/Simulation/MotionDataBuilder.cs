using Unity.Mathematics;

namespace CoinFlight
{
    /// <summary>
    /// Предвычисляет per-coin <see cref="CoinMotionData"/> во время <c>Play()</c>.
    /// Все случайные смещения (контрольная точка Bezier, направление Scatter)
    /// берутся из детерминированных per-coin seed-ов (план §14.4), чтобы
    /// evaluator оставался pure и Burst-friendly.
    /// </summary>
    public static class MotionDataBuilder
    {
        public static void Build(
            in CoinFlightRequest request,
            float2 startCanvas,
            float2 endCanvas,
            CoinFlightConfig config,
            CoinMotionData[] output,
            int coinCount)
        {
            float arcHeight = config.DefaultArcHeight * math.max(0f, request.PatternStrength);
            float2 arcAxis = new float2(0f, 1f);
            float scatterEnd = math.clamp(config.ScatterEndT, 0.01f, 0.99f);

            float2 mid = (startCanvas + endCanvas) * 0.5f;
            float2 dir = endCanvas - startCanvas;
            float len = math.length(dir);
            float2 perp = len > 1e-5f ? new float2(-dir.y, dir.x) / len : new float2(0f, 1f);
            float bezierStrength = config.DefaultBezierStrength * math.max(0f, request.PatternStrength);
            float scatterRadius = config.ScatterRadius * math.max(0f, request.PatternStrength);

            for (int i = 0; i < coinCount; i++)
            {
                uint seed = WangHash.Derive(request.RandomSeed, (uint)i);
                var rng = new Random(seed);

                float2 control = default;
                float2 scatter = default;

                switch (request.Pattern)
                {
                    case MotionPattern.Bezier:
                    {
                        float jitter = rng.NextFloat(-1f, 1f);
                        control = mid + perp * (bezierStrength * jitter);
                        break;
                    }
                    case MotionPattern.ScatterCollect:
                    {
                        float angle = rng.NextFloat(0f, math.PI * 2f);
                        float radius = rng.NextFloat(0.3f, 1f) * scatterRadius;
                        scatter = mid + new float2(math.cos(angle), math.sin(angle)) * radius;
                        break;
                    }
                }

                output[i] = new CoinMotionData(
                    startPos: startCanvas,
                    endPos: endCanvas,
                    controlPoint: control,
                    scatterPoint: scatter,
                    arcHeight: arcHeight,
                    arcAxis: arcAxis,
                    scatterEnd: scatterEnd,
                    pattern: request.Pattern,
                    easing: request.Easing,
                    seed: seed);
            }
        }
    }
}
