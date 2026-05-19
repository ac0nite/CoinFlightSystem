namespace CoinFlight
{
    /// <summary>
    /// Детерминированный хелпер для деривации seed-ов. Комбинирует глобальный seed
    /// и индекс монеты в per-coin ненулевой seed, подходящий для
    /// <c>Unity.Mathematics.Random</c>. Вызывается во время <c>Play()</c>
    /// (не в hot path evaluator-а).
    /// </summary>
    public static class WangHash
    {
        public static uint Derive(uint seed, uint index)
        {
            uint x = seed + index * 0x9E3779B9u;
            x = (x ^ 61u) ^ (x >> 16);
            x += x << 3;
            x ^= x >> 4;
            x *= 0x27D4EB2Du;
            x ^= x >> 15;
            return x == 0u ? 1u : x;
        }
    }
}
