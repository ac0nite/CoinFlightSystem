using NUnit.Framework;
using Unity.Mathematics;

namespace CoinFlight.Tests
{
    [TestFixture]
    public class MotionPatternEvaluatorTests
    {
        private const float Eps = 1e-4f;

        private static readonly float2 Start = new float2(0f, 0f);
        private static readonly float2 End = new float2(100f, 50f);
        private static readonly float2 Control = new float2(50f, 200f);
        private static readonly float2 Scatter = new float2(20f, 120f);

        private static CoinMotionData Data(MotionPattern p, EasingType e = EasingType.Linear)
        {
            return new CoinMotionData(
                startPos: Start,
                endPos: End,
                controlPoint: Control,
                scatterPoint: Scatter,
                arcHeight: 60f,
                arcAxis: new float2(0f, 1f),
                scatterEnd: 0.35f,
                pattern: p,
                easing: e,
                seed: 1u);
        }

        private static readonly MotionPattern[] AllPatterns =
        {
            MotionPattern.Linear,
            MotionPattern.Arc,
            MotionPattern.Bezier,
            MotionPattern.ScatterCollect
        };

        [Test]
        [Description("При t=0 любой паттерн движения обязан возвращать точно StartPos (snap, без смещений).")]
        public void Evaluate_AtZero_IsStartPos_ForAllPatterns()
        {
            foreach (var p in AllPatterns)
            {
                var s = MotionPatternEvaluator.Evaluate(Data(p), 0f);
                Assert.AreEqual(Start.x, s.Position.x, Eps, $"pattern={p} x");
                Assert.AreEqual(Start.y, s.Position.y, Eps, $"pattern={p} y");
            }
        }

        [Test]
        [Description("При t=1 любой паттерн обязан возвращать точно EndPos (snap, без scatter/random смещений).")]
        public void Evaluate_AtOne_IsEndPos_ForAllPatterns()
        {
            foreach (var p in AllPatterns)
            {
                var s = MotionPatternEvaluator.Evaluate(Data(p), 1f);
                Assert.AreEqual(End.x, s.Position.x, Eps, $"pattern={p} x");
                Assert.AreEqual(End.y, s.Position.y, Eps, $"pattern={p} y");
            }
        }

        [Test]
        [Description("Linear паттерн при t=0.5 даёт геометрическую середину между Start и End.")]
        public void Linear_AtHalf_IsMidpoint()
        {
            var s = MotionPatternEvaluator.Evaluate(Data(MotionPattern.Linear), 0.5f);
            Assert.AreEqual(50f, s.Position.x, Eps);
            Assert.AreEqual(25f, s.Position.y, Eps);
        }

        [Test]
        [Description("Arc в вершине дуги (t=0.5) = linear-точка + ArcHeight вдоль ArcAxis, т.к. sin(PI*0.5)=1.")]
        public void Arc_AtHalf_IsLinearPlusPeakHeight()
        {
            var s = MotionPatternEvaluator.Evaluate(Data(MotionPattern.Arc), 0.5f);
            // Linear mid = (50, 25); sin(PI * 0.5) = 1 => +ArcHeight * ArcAxis(0,1) = (0, 60)
            Assert.AreEqual(50f, s.Position.x, Eps);
            Assert.AreEqual(25f + 60f, s.Position.y, Eps);
        }

        [Test]
        [Description("Bezier при t=0.5 совпадает с формулой 0.25*Start + 0.5*Control + 0.25*End.")]
        public void Bezier_AtHalf_EqualsQuadraticFormula()
        {
            var s = MotionPatternEvaluator.Evaluate(Data(MotionPattern.Bezier), 0.5f);
            // B(0.5) = 0.25 * Start + 0.5 * Control + 0.25 * End
            float2 expected = 0.25f * Start + 0.5f * Control + 0.25f * End;
            Assert.AreEqual(expected.x, s.Position.x, Eps);
            Assert.AreEqual(expected.y, s.Position.y, Eps);
        }

        [Test]
        [Description("ScatterCollect в точке стыка фаз (t=ScatterEnd) должен находиться точно в ScatterPoint.")]
        public void ScatterCollect_AtScatterEnd_IsScatterPoint()
        {
            var s = MotionPatternEvaluator.Evaluate(Data(MotionPattern.ScatterCollect), 0.35f);
            Assert.AreEqual(Scatter.x, s.Position.x, Eps);
            Assert.AreEqual(Scatter.y, s.Position.y, Eps);
        }

        [Test]
        [Description("На первой фазе (t < ScatterEnd) ScatterCollect линейно интерполирует от Start к ScatterPoint.")]
        public void ScatterCollect_BelowScatterEnd_InterpolatesStartToScatter()
        {
            // t = 0.175 => localT = 0.5 in first phase
            var s = MotionPatternEvaluator.Evaluate(Data(MotionPattern.ScatterCollect), 0.175f);
            float2 expected = math.lerp(Start, Scatter, 0.5f);
            Assert.AreEqual(expected.x, s.Position.x, Eps);
            Assert.AreEqual(expected.y, s.Position.y, Eps);
        }

        [Test]
        [Description("Повторный вызов Evaluate с одинаковыми (data, t) должен давать бит-в-бит идентичный результат (детерминизм).")]
        public void Evaluate_IsDeterministic()
        {
            var d = Data(MotionPattern.Bezier, EasingType.OutCubic);
            var a = MotionPatternEvaluator.Evaluate(d, 0.37f);
            var b = MotionPatternEvaluator.Evaluate(d, 0.37f);
            Assert.AreEqual(a.Position.x, b.Position.x);
            Assert.AreEqual(a.Position.y, b.Position.y);
        }

        [Test]
        [Description("В полете монета всегда выставляет флаг Visible в возвращаемом CoinVisualState.")]
        public void Evaluate_ReturnsVisibleFlag()
        {
            var s = MotionPatternEvaluator.Evaluate(Data(MotionPattern.Linear), 0.5f);
            Assert.IsTrue((s.Flags & CoinVisualFlags.Visible) != 0);
        }
    }
}
