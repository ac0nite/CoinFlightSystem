using NUnit.Framework;

namespace CoinFlight.Tests
{
    [TestFixture]
    public class EasingEvaluatorTests
    {
        private const float Eps = 1e-5f;

        private static readonly EasingType[] AllEasings =
        {
            EasingType.Linear,
            EasingType.InQuad,
            EasingType.OutQuad,
            EasingType.InOutQuad,
            EasingType.OutCubic,
            EasingType.OutBack
        };

        [Test]
        [Description("Все функции сглаживания в точке t=0 должны возвращать ровно 0.")]
        public void Evaluate_AtZero_IsZero_ForAllEasings()
        {
            foreach (var e in AllEasings)
                Assert.AreEqual(0f, EasingEvaluator.Evaluate(e, 0f), Eps, $"easing={e}");
        }

        [Test]
        [Description("Все функции сглаживания в точке t=1 должны возвращать ровно 1.")]
        public void Evaluate_AtOne_IsOne_ForAllEasings()
        {
            foreach (var e in AllEasings)
                Assert.AreEqual(1f, EasingEvaluator.Evaluate(e, 1f), Eps, $"easing={e}");
        }

        [Test]
        [Description("Linear должен вести себя как тождественная функция f(t) = t.")]
        public void Linear_IsIdentity()
        {
            Assert.AreEqual(0.25f, EasingEvaluator.Evaluate(EasingType.Linear, 0.25f), Eps);
            Assert.AreEqual(0.50f, EasingEvaluator.Evaluate(EasingType.Linear, 0.50f), Eps);
            Assert.AreEqual(0.75f, EasingEvaluator.Evaluate(EasingType.Linear, 0.75f), Eps);
        }

        [Test]
        [Description("InQuad в точке t=0.5 должен возвращать 0.25 (0.5^2).")]
        public void InQuad_HalfValueIsQuarter()
        {
            Assert.AreEqual(0.25f, EasingEvaluator.Evaluate(EasingType.InQuad, 0.5f), Eps);
        }

        [Test]
        [Description("OutQuad в точке t=0.5 должен возвращать 0.75 (зеркальный InQuad).")]
        public void OutQuad_HalfValueIsThreeQuarters()
        {
            Assert.AreEqual(0.75f, EasingEvaluator.Evaluate(EasingType.OutQuad, 0.5f), Eps);
        }

        [Test]
        [Description("InOutQuad в середине (t=0.5) должен давать 0.5 (точка склейки двух фаз).")]
        public void InOutQuad_HalfValueIsOneHalf()
        {
            Assert.AreEqual(0.5f, EasingEvaluator.Evaluate(EasingType.InOutQuad, 0.5f), Eps);
        }

        [Test]
        [Description("OutBack обязан overshoot-ить выше 1 на участке (0,1) перед возвратом к 1.")]
        public void OutBack_Overshoots_BeyondOne_InMidRange()
        {
            // OutBack должен в какой-то момент превышать 1 (overshoot) перед возвратом к 1.
            bool overshot = false;
            for (int i = 1; i < 100; i++)
            {
                float t = i / 100f;
                if (EasingEvaluator.Evaluate(EasingType.OutBack, t) > 1f)
                {
                    overshot = true;
                    break;
                }
            }
            Assert.IsTrue(overshot, "OutBack must overshoot past 1 before landing.");
        }
    }
}
