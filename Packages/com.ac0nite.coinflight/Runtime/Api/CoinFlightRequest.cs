using UnityEngine;

namespace CoinFlight
{
    /// <summary>
    /// Неизменяемое описание одного батча полёта монет. Передаётся через
    /// <c>in</c>, чтобы избежать копирования.
    /// </summary>
    public readonly struct CoinFlightRequest
    {
        public readonly CoinSource Source;
        public readonly CoinTarget Target;
        public readonly int Count;
        public readonly float Duration;
        public readonly float StartDelay;
        public readonly float StaggerPerCoin;
        public readonly MotionPattern Pattern;
        public readonly EasingType Easing;
        public readonly float PatternStrength;
        public readonly int PrefabId;
        public readonly Sprite Sprite;
        public readonly Sprite[] RandomSprites;
        public readonly uint RandomSeed;
        public readonly TimeMode TimeMode;

        public CoinFlightRequest(
            CoinSource source,
            CoinTarget target,
            int count,
            float duration,
            MotionPattern pattern = MotionPattern.Arc,
            EasingType easing = EasingType.OutCubic,
            float patternStrength = 1f,
            float startDelay = 0f,
            float staggerPerCoin = 0f,
            int prefabId = 0,
            Sprite sprite = null,
            Sprite[] randomSprites = null,
            uint randomSeed = 1u,
            TimeMode timeMode = TimeMode.Scaled)
        {
            Source = source;
            Target = target;
            Count = count;
            Duration = duration;
            StartDelay = startDelay;
            StaggerPerCoin = staggerPerCoin;
            Pattern = pattern;
            Easing = easing;
            PatternStrength = patternStrength;
            PrefabId = prefabId;
            Sprite = sprite;
            RandomSprites = randomSprites;
            RandomSeed = randomSeed;
            TimeMode = timeMode;
        }
    }
}
