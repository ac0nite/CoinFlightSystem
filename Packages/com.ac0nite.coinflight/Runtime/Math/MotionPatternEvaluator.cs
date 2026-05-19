using Unity.Mathematics;

namespace CoinFlight
{
    /// <summary>
    /// Чистый, stateless, allocation-free evaluator паттернов движения.
    /// Возвращает <see cref="CoinVisualState"/> для заданных предвычисленных
    /// <see cref="CoinMotionData"/> и нормализованного времени <c>t</c>.
    /// Контракт (план §14.8):
    ///   - pure / stateless / детерминированный;
    ///   - <c>t = 0</c> => позиция строго в <c>StartPos</c>;
    ///   - <c>t = 1</c> => позиция строго в <c>EndPos</c> (snap, без random-смещений);
    ///   - evaluator НЕ делает clamp <c>t</c>; это ответственность драйвера.
    /// </summary>
    public static class MotionPatternEvaluator
    {
        public static CoinVisualState Evaluate(in CoinMotionData data, float t)
        {
            float eased = EasingEvaluator.Evaluate(data.Easing, t);
            float2 position;

            switch (data.Pattern)
            {
                case MotionPattern.Linear:
                    position = math.lerp(data.StartPos, data.EndPos, eased);
                    break;

                case MotionPattern.Arc:
                {
                    float2 linear = math.lerp(data.StartPos, data.EndPos, eased);
                    float2 arc = data.ArcAxis * (math.sin(math.PI * eased) * data.ArcHeight);
                    position = linear + arc;
                    break;
                }

                case MotionPattern.Bezier:
                {
                    float u = 1f - eased;
                    position = (u * u) * data.StartPos
                             + (2f * u * eased) * data.ControlPoint
                             + (eased * eased) * data.EndPos;
                    break;
                }

                case MotionPattern.ScatterCollect:
                {
                    float scatterEnd = data.ScatterEnd;
                    if (eased < scatterEnd)
                    {
                        float localT = scatterEnd > 0f ? eased / scatterEnd : 0f;
                        position = math.lerp(data.StartPos, data.ScatterPoint, localT);
                    }
                    else
                    {
                        float span = 1f - scatterEnd;
                        float localT = span > 0f ? (eased - scatterEnd) / span : 1f;
                        position = math.lerp(data.ScatterPoint, data.EndPos, localT);
                    }
                    break;
                }

                default:
                    position = data.EndPos;
                    break;
            }

            return new CoinVisualState(
                position,
                rotationDeg: 0f,
                scale: 1f,
                alpha: 1f,
                flags: CoinVisualFlags.Visible);
        }
    }
}
