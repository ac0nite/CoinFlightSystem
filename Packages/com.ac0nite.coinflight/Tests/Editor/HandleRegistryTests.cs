using NUnit.Framework;

namespace CoinFlight.Tests
{
    [TestFixture]
    public class HandleRegistryTests
    {
        private sealed class StubGroup : ICoinFlightGroup
        {
            public bool IsAlive { get; set; } = true;
            public int CoinCount => 1;
            public int StopCallCount { get; private set; }
            public CancelPolicy LastPolicy { get; private set; }

            public void Stop(CancelPolicy policy)
            {
                StopCallCount++;
                LastPolicy = policy;
                IsAlive = false;
            }
        }

        [Test]
        [Description("Register возвращает валидный handle; TryGet находит зарегистрированную группу.")]
        public void Register_IssuesValidHandle_And_TryGetResolves()
        {
            var reg = new HandleRegistry(max: 4);
            var g = new StubGroup();

            var h = reg.Register(g);

            Assert.IsTrue(h.IsValid);
            Assert.IsTrue(reg.TryGet(h, out var resolved));
            Assert.AreSame(g, resolved);
        }

        [Test]
        [Description("Unregister инвалидирует старый handle (generation bump), TryGet по нему возвращает false.")]
        public void Unregister_InvalidatesOldHandle_ViaGeneration()
        {
            var reg = new HandleRegistry(max: 4);
            var h1 = reg.Register(new StubGroup());
            reg.Unregister(h1);

            Assert.IsFalse(reg.TryGet(h1, out _));

            // Новая регистрация может занять тот же слот, но с новой generation.
            var h2 = reg.Register(new StubGroup());
            Assert.IsTrue(h2.IsValid);
            Assert.AreNotEqual(h1.Generation, h2.Generation);
            Assert.IsFalse(reg.TryGet(h1, out _), "Старый handle должен оставаться невалидным даже после reuse слота.");
        }

        [Test]
        [Description("Default OverflowPolicy.DropNew: при переполнении Register возвращает Invalid.")]
        public void Register_OverflowDropNew_ReturnsInvalid()
        {
            var reg = new HandleRegistry(max: 2);
            Assert.IsTrue(reg.Register(new StubGroup()).IsValid);
            Assert.IsTrue(reg.Register(new StubGroup()).IsValid);
            Assert.IsFalse(reg.Register(new StubGroup()).IsValid);
        }

        [Test]
        [Description("OverflowPolicy.Expand: при переполнении ёмкость удваивается, Register успешен.")]
        public void Register_OverflowExpand_GrowsCapacity()
        {
            var reg = new HandleRegistry(max: 2, overflow: OverflowPolicy.Expand);
            reg.Register(new StubGroup());
            reg.Register(new StubGroup());
            var h = reg.Register(new StubGroup());
            Assert.IsTrue(h.IsValid);
            Assert.IsTrue(reg.Max >= 3);
        }

        [Test]
        [Description("StopAll вызывает Stop на каждой живой группе и передаёт заданный CancelPolicy.")]
        public void StopAll_CallsStopOnAllLiveGroups()
        {
            var reg = new HandleRegistry(max: 4);
            var a = new StubGroup();
            var b = new StubGroup();
            reg.Register(a);
            reg.Register(b);

            reg.StopAll(CancelPolicy.SnapToTarget);

            Assert.AreEqual(1, a.StopCallCount);
            Assert.AreEqual(1, b.StopCallCount);
            Assert.AreEqual(CancelPolicy.SnapToTarget, a.LastPolicy);
            Assert.AreEqual(CancelPolicy.SnapToTarget, b.LastPolicy);
        }

        [Test]
        [Description("TryGet для CoinFlightHandle.Invalid должен безопасно возвращать false.")]
        public void TryGet_InvalidHandle_ReturnsFalse()
        {
            var reg = new HandleRegistry(max: 4);
            Assert.IsFalse(reg.TryGet(CoinFlightHandle.Invalid, out _));
        }
    }
}
