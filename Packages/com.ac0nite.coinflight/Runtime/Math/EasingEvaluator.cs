using Unity.Mathematics;

namespace CoinFlight
{
    /// <summary>
    /// Чистый, stateless, Burst-совместимый evaluator функций сглаживания.
    /// Контракт: <c>f(0) = 0</c>, <c>f(1) = 1</c> для всех поддерживаемых easing-ов.
    /// Драйвер отвечает за clamping <c>t</c>; evaluator сам не делает clamp.
    /// </summary>
    public static class EasingEvaluator
    {
        private const float BackC1 = 1.70158f;
        private const float BackC3 = BackC1 + 1f;

        public static float Evaluate(EasingType type, float t)
        {
            switch (type)
            {
                case EasingType.Linear:
                    return t;
                case EasingType.InQuad:
                    return t * t;
                case EasingType.OutQuad:
                {
                    float u = 1f - t;
                    return 1f - u * u;
                }
                case EasingType.InOutQuad:
                    return t < 0.5f
                        ? 2f * t * t
                        : 1f - 0.5f * math.pow(-2f * t + 2f, 2f);
                case EasingType.OutCubic:
                {
                    float u = 1f - t;
                    return 1f - u * u * u;
                }
                case EasingType.OutBack:
                {
                    float u = t - 1f;
                    return 1f + BackC3 * u * u * u + BackC1 * u * u;
                }
                default:
                    return t;
            }
        }
    }
}
