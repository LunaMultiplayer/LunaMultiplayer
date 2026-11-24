using Microsoft.VisualStudio.TestTools.UnitTesting;
using LmpClient.Systems.Scenario;
using System.Collections.Generic;

namespace ServerTest
{
    [TestClass]
    public class DeployedScienceSyncGateTest
    {
        private const double Grace = 5.0;

        #region Send gate decisions

        [TestMethod]
        public void DeployedScienceSendBlockedUntilAuthoritativeApplySucceeded()
        {
            DeployedScienceSyncGate.DeployedScienceReady = false;
            Assert.IsFalse(DeployedScienceSyncGate.ShouldSend(DeployedScienceSyncGate.ScenarioModuleName));
        }

        [TestMethod]
        public void DeployedScienceSendAllowedAfterSuccessfulApply()
        {
            DeployedScienceSyncGate.DeployedScienceReady = true;
            Assert.IsTrue(DeployedScienceSyncGate.ShouldSend(DeployedScienceSyncGate.ScenarioModuleName));
        }

        [TestMethod]
        public void DeployedScienceSendRemainsBlockedWhileApplyFailedOrRetrying()
        {
            DeployedScienceSyncGate.DeployedScienceReady = false;
            Assert.IsFalse(DeployedScienceSyncGate.ShouldSend(DeployedScienceSyncGate.ScenarioModuleName));
        }

        [TestMethod]
        public void NonDeployedScienceScenariosAlwaysSendable()
        {
            DeployedScienceSyncGate.DeployedScienceReady = false;
            Assert.IsTrue(DeployedScienceSyncGate.ShouldSend("ContractSystem"));
            Assert.IsTrue(DeployedScienceSyncGate.ShouldSend("ProgressTracking"));
            Assert.IsTrue(DeployedScienceSyncGate.ShouldSend(null));
        }

        #endregion

        #region Fresh-server unblock

        [TestMethod]
        public void FreshServerUnblocksSendOnceVesselSyncCompletes()
        {
            Assert.IsTrue(DeployedScienceSyncGate.ShouldMarkFreshModuleAuthoritative(
                receivedScenarioEntry: false, vesselSyncComplete: true, alreadyReady: false));
        }

        [TestMethod]
        public void FreshServerUnblockNeverFiresAfterAuthoritativeEntryReceived()
        {
            Assert.IsFalse(DeployedScienceSyncGate.ShouldMarkFreshModuleAuthoritative(
                receivedScenarioEntry: true, vesselSyncComplete: true, alreadyReady: false));
        }

        [TestMethod]
        public void FreshServerUnblockWaitsForVesselSyncCompletion()
        {
            Assert.IsFalse(DeployedScienceSyncGate.ShouldMarkFreshModuleAuthoritative(
                receivedScenarioEntry: false, vesselSyncComplete: false, alreadyReady: false));
        }

        [TestMethod]
        public void FreshServerUnblockDoesNotReFireOnceReady()
        {
            Assert.IsFalse(DeployedScienceSyncGate.ShouldMarkFreshModuleAuthoritative(
                receivedScenarioEntry: false, vesselSyncComplete: true, alreadyReady: true));
        }

        #endregion

        #region Cluster membership apply check

        private static DeployedScienceSyncGate.ClusterMembership Membership(uint controlId, uint[] partIds, string[] experimentIds = null)
        {
            var m = new DeployedScienceSyncGate.ClusterMembership();
            foreach (var pid in partIds) m.PartIds.Add(pid);
            if (experimentIds != null)
                foreach (var eid in experimentIds) m.ExperimentIds.Add(eid);
            return m;
        }

        [TestMethod]
        public void MembershipEqualWhenClustersPartsAndExperimentsMatch()
        {
            var declared = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 10u, 11u }, new[] { "expA" }) },
                { 2u, Membership(2u, new[] { 20u }) },
            };
            var applied = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 11u, 10u }, new[] { "expA" }) }, // order-independent
                { 2u, Membership(2u, new[] { 20u }) },
            };
            Assert.IsTrue(DeployedScienceSyncGate.IsMembershipApplied(declared, applied));
        }

        [TestMethod]
        public void EmptyMembershipsAreApplied()
        {
            var declared = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>();
            var applied = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>();
            Assert.IsTrue(DeployedScienceSyncGate.IsMembershipApplied(declared, applied));
        }

        [TestMethod]
        public void MissingClusterFailsMembership()
        {
            var declared = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 10u }) },
                { 2u, Membership(2u, new[] { 20u }) },
            };
            var applied = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 10u }) },
            };
            Assert.IsFalse(DeployedScienceSyncGate.IsMembershipApplied(declared, applied));
        }

        [TestMethod]
        public void PrunedPartInsideClusterFailsMembership()
        {
            var declared = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 10u, 11u, 12u }, new[] { "expA" }) },
            };
            var applied = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 10u, 12u }, new[] { "expA" }) }, // 11u pruned
            };
            Assert.IsFalse(DeployedScienceSyncGate.IsMembershipApplied(declared, applied));
        }

        [TestMethod]
        public void PrunedExperimentInsideClusterFailsMembership()
        {
            var declared = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 10u, 11u }, new[] { "expA", "expB" }) },
            };
            var applied = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 10u, 11u }, new[] { "expA" }) }, // expB pruned
            };
            Assert.IsFalse(DeployedScienceSyncGate.IsMembershipApplied(declared, applied));
        }

        [TestMethod]
        public void ExtraPartInsideClusterFailsMembership()
        {
            var declared = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 10u }) },
            };
            var applied = new Dictionary<uint, DeployedScienceSyncGate.ClusterMembership>
            {
                { 1u, Membership(1u, new[] { 10u, 99u }) },
            };
            Assert.IsFalse(DeployedScienceSyncGate.IsMembershipApplied(declared, applied));
        }

        #endregion

        #region VesselSyncCompletionTracker

        private static VesselSyncCompletionTracker CreateTracker() => new VesselSyncCompletionTracker(Grace);

        [TestMethod]
        public void TrackerDoesNothingWhileSceneNotReady()
        {
            var tracker = CreateTracker();
            tracker.Update(10.0, protoSystemReady: false, anyProtoPending: true, vesselLoadedThisTick: false);
            tracker.Update(11.0, protoSystemReady: false, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsFalse(tracker.IsComplete);
        }

        [TestMethod]
        public void EmptyServerCompletesOnlyAfterGraceWindow()
        {
            var tracker = CreateTracker();
            tracker.Update(10.0, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsFalse(tracker.IsComplete, "must not complete before the grace window elapses");

            tracker.Update(10.0 + Grace - 0.1, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsFalse(tracker.IsComplete);

            tracker.Update(10.0 + Grace, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsTrue(tracker.IsComplete);
        }

        [TestMethod]
        public void ReadinessDropRestartsEmptyServerWindow()
        {
            var tracker = CreateTracker();
            tracker.Update(10.0, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false); // window starts at 10
            tracker.Update(12.0, protoSystemReady: false, anyProtoPending: false, vesselLoadedThisTick: false); // readiness dropped
            tracker.Update(20.0, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false); // window restarts at 20
            tracker.Update(20.0 + Grace - 0.1, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsFalse(tracker.IsComplete, "window must restart after a readiness drop");
            tracker.Update(20.0 + Grace, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsTrue(tracker.IsComplete);
        }

        [TestMethod]
        public void PendingProtosDelayCompletionAndRestartGrace()
        {
            var tracker = CreateTracker();
            tracker.Update(10.0, protoSystemReady: true, anyProtoPending: true, vesselLoadedThisTick: false);
            tracker.Update(10.0 + Grace - 0.1, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsFalse(tracker.IsComplete, "grace must be measured from the last activity");

            tracker.Update(10.0 + Grace + 1.0, protoSystemReady: true, anyProtoPending: true, vesselLoadedThisTick: false);
            tracker.Update(10.0 + Grace + 1.0 + Grace - 0.1, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsFalse(tracker.IsComplete);

            tracker.Update(10.0 + Grace + 1.0 + Grace, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsTrue(tracker.IsComplete);
        }

        [TestMethod]
        public void VesselLoadCountsAsActivity()
        {
            var tracker = CreateTracker();
            tracker.Update(10.0, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: true);
            tracker.Update(10.0 + Grace - 0.1, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsFalse(tracker.IsComplete);
            tracker.Update(10.0 + Grace, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsTrue(tracker.IsComplete);
        }

        [TestMethod]
        public void TrackerLatchesOnceComplete()
        {
            var tracker = CreateTracker();
            tracker.Update(10.0, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            tracker.Update(10.0 + Grace, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsTrue(tracker.IsComplete);

            tracker.Update(50.0, protoSystemReady: true, anyProtoPending: true, vesselLoadedThisTick: true);
            Assert.IsTrue(tracker.IsComplete);
        }

        [TestMethod]
        public void ResetReArmsTracker()
        {
            var tracker = CreateTracker();
            tracker.Update(10.0, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            tracker.Update(10.0 + Grace, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsTrue(tracker.IsComplete);

            tracker.Reset();
            Assert.IsFalse(tracker.IsComplete);

            tracker.Update(20.0, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            tracker.Update(20.0 + Grace - 0.1, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsFalse(tracker.IsComplete);
            tracker.Update(20.0 + Grace, protoSystemReady: true, anyProtoPending: false, vesselLoadedThisTick: false);
            Assert.IsTrue(tracker.IsComplete);
        }

        #endregion
    }
}
