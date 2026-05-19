using NUnit.Framework;

namespace CoinFlight.Tests
{
    [TestFixture]
    public class WangHashTests
    {
        [Test]
        [Description("WangHash.Derive детерминистичен: одинаковые (seed, index) дают одинаковый вывод.")]
        public void Derive_IsDeterministic_ForSameInputs()
        {
            Assert.AreEqual(WangHash.Derive(42u, 7u), WangHash.Derive(42u, 7u));
        }

        [Test]
        [Description("Разные coin индексы должны давать разные seed-значения (разведение траекторий внутри одного батча).")]
        public void Derive_DiffersForDifferentIndices()
        {
            uint a = WangHash.Derive(42u, 7u);
            uint b = WangHash.Derive(42u, 8u);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        [Description("Разные request.RandomSeed должны давать разные per-coin seeds (разные батчи дают разные траектории).")]
        public void Derive_DiffersForDifferentSeeds()
        {
            uint a = WangHash.Derive(42u, 7u);
            uint b = WangHash.Derive(43u, 7u);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        [Description("Derive никогда не возвращает 0 — Unity.Mathematics.Random требует ненулевой seed.")]
        public void Derive_NeverReturnsZero()
        {
            // Unity.Mathematics.Random requires a non-zero seed.
            for (uint i = 0; i < 1000u; i++)
                Assert.AreNotEqual(0u, WangHash.Derive(0u, i));
        }
    }
}
