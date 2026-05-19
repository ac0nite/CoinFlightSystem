using System;

namespace CoinFlight
{
    /// <summary>
    /// Непрозрачный идентификатор активной группы монет. Использует пару
    /// (id, generation), чтобы устаревшие хэндлы не могли случайно адресовать
    /// повторно используемые слоты.
    /// </summary>
    public readonly struct CoinFlightHandle : IEquatable<CoinFlightHandle>
    {
        public static readonly CoinFlightHandle Invalid = default;

        public readonly int Id;
        public readonly int Generation;

        public CoinFlightHandle(int id, int generation)
        {
            Id = id;
            Generation = generation;
        }

        public bool IsValid => Id != 0 || Generation != 0;

        public bool Equals(CoinFlightHandle other) => Id == other.Id && Generation == other.Generation;
        public override bool Equals(object obj) => obj is CoinFlightHandle h && Equals(h);
        public override int GetHashCode() => unchecked((Id * 397) ^ Generation);

        public static bool operator ==(CoinFlightHandle a, CoinFlightHandle b) => a.Equals(b);
        public static bool operator !=(CoinFlightHandle a, CoinFlightHandle b) => !a.Equals(b);
    }
}
