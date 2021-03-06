using System;
using System.Linq;
using Xunit;

namespace Ducia.Tests.Layers.Mind {
    public class MindTests : IDisposable {
        private BasicMind mind;

        public MindTests() {
            mind = new BasicMind();
            // use single thread for tests
            BasicMind.useThreadPool = false;
            mind.initialize();
        }

        [Fact]
        public void canConstructMind() {
            Assert.NotNull(mind.state);
        }

        [Fact]
        public void canAddSystems() {
            Assert.NotEmpty(mind.sensorySystems);
            Assert.NotEmpty(mind.cognitiveSystems);
        }

        /// <summary>
        /// ensure that systems can be updated
        /// </summary>
        [Fact]
        public void canUpdateSystems() {
            mind.tick(0.1f);
            var rhythmSystem = (BasicMind.RhythmSystem) mind.sensorySystems.Single(x => x is BasicMind.RhythmSystem);
            Assert.True(rhythmSystem.beats > 0);
            var rngSystem =
                (BasicMind.RandomSeedSystem) mind.cognitiveSystems.Single(x => x is BasicMind.RandomSeedSystem);
            Assert.True(rngSystem.state >= 0);
        }

        /// <summary>
        /// ensure that systems don't update faster than their interval
        /// </summary>
        [Fact]
        public void systemUpdateThrottled() {
            mind.tick(0.1f);
            var rhythmSystem = (BasicMind.RhythmSystem) mind.sensorySystems.Single(x => x is BasicMind.RhythmSystem);
            Assert.True(rhythmSystem.beats == 1);
            mind.tick(0.5f);
            Assert.True(rhythmSystem.beats == 2);
        }

        [Fact]
        public void canPropagateSignals() {
            var taxSystem = (BasicMind.TaxReturnsSystem) mind.cognitiveSystems.Single(x => x is BasicMind.TaxReturnsSystem);
            Assert.Equal(0, taxSystem.paid);
            // send a signal
            mind.signal(new BasicMind.BillSignal(10));
            // respond to the signal
            mind.tick(0.1f);
            Assert.Equal(10, taxSystem.paid);
        }

        [Fact]
        public void canCompleteTasks() {
            // add plan to task
            mind.state.plan.Enqueue(new BasicMind.PushButtonTask(mind));
            Assert.True(mind.state.plan.TryPeek(out var task));
            var btnTask = task as BasicMind.PushButtonTask;
            Assert.Equal(PlanTask.Status.Ongoing, btnTask.status());
            btnTask.press();
            Assert.Equal(PlanTask.Status.Complete, btnTask.status());
        }

        [Fact]
        public void tasksCanExpire() {
            // add plan to task
            mind.state.plan.Enqueue(new BasicMind.PushButtonTask(mind, 1f));
            Assert.True(mind.state.plan.TryPeek(out var task));
            var btnTask = task as BasicMind.PushButtonTask;
            Assert.Equal(PlanTask.Status.Ongoing, btnTask.status());
            mind.tick(0.5f);
            Assert.Equal(PlanTask.Status.Ongoing, btnTask.status());
            mind.tick(0.5f);
            Assert.Equal(PlanTask.Status.Failed, btnTask.status());
        }

        public void Dispose() {
            mind.destroy();
        }
    }
}