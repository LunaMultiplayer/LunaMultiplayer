using System.Collections.Generic;
using LmpClient.VesselUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerTest
{
    [TestClass]
    public class PersistentIdEvictionPolicyTest
    {
        [TestMethod]
        public void UnregisteredIdIsNotTouched()
        {
            Assert.AreEqual(PersistentIdEvictionDecision.NotRegistered,
                PersistentIdEvictionPolicy.DecideUnloaded(registered: false, ownerSnapshotNull: false, ownerVesselNull: false, ownerIsIncomingVessel: false));
            Assert.AreEqual(PersistentIdEvictionDecision.NotRegistered,
                PersistentIdEvictionPolicy.DecideLoaded(registered: false, ownerPartNull: false, ownerVesselNull: false, ownerIsIncomingVessel: false));
        }

        [TestMethod]
        public void NullRegistryEntriesAreStale()
        {
            Assert.AreEqual(PersistentIdEvictionDecision.EvictStaleOwner,
                PersistentIdEvictionPolicy.DecideUnloaded(registered: true, ownerSnapshotNull: true, ownerVesselNull: false, ownerIsIncomingVessel: false));
            Assert.AreEqual(PersistentIdEvictionDecision.EvictStaleOwner,
                PersistentIdEvictionPolicy.DecideUnloaded(registered: true, ownerSnapshotNull: false, ownerVesselNull: true, ownerIsIncomingVessel: false));
            Assert.AreEqual(PersistentIdEvictionDecision.EvictStaleOwner,
                PersistentIdEvictionPolicy.DecideLoaded(registered: true, ownerPartNull: true, ownerVesselNull: false, ownerIsIncomingVessel: false));
            Assert.AreEqual(PersistentIdEvictionDecision.EvictStaleOwner,
                PersistentIdEvictionPolicy.DecideLoaded(registered: true, ownerPartNull: false, ownerVesselNull: true, ownerIsIncomingVessel: false));
        }

        [TestMethod]
        public void OwnPreviousCopyIsEvictedOnReconnect()
        {
            Assert.AreEqual(PersistentIdEvictionDecision.EvictStaleOwner,
                PersistentIdEvictionPolicy.DecideUnloaded(registered: true, ownerSnapshotNull: false, ownerVesselNull: false, ownerIsIncomingVessel: true));
            Assert.AreEqual(PersistentIdEvictionDecision.EvictStaleOwner,
                PersistentIdEvictionPolicy.DecideLoaded(registered: true, ownerPartNull: false, ownerVesselNull: false, ownerIsIncomingVessel: true));
        }

        [TestMethod]
        public void LiveForeignOwnerIsKeptForStockRenaming()
        {
            Assert.AreEqual(PersistentIdEvictionDecision.KeepForeignOwner,
                PersistentIdEvictionPolicy.DecideUnloaded(registered: true, ownerSnapshotNull: false, ownerVesselNull: false, ownerIsIncomingVessel: false));
            Assert.AreEqual(PersistentIdEvictionDecision.KeepForeignOwner,
                PersistentIdEvictionPolicy.DecideLoaded(registered: true, ownerPartNull: false, ownerVesselNull: false, ownerIsIncomingVessel: false));
        }

        [TestMethod]
        public void LoadedOwnerRestoreRequiresVesselPresenceInFlightGlobals()
        {
            Assert.IsFalse(PersistentIdEvictionPolicy.ShouldRestoreLoadedOwner(
                ownerVesselStillPresent: false, idAlreadyRegistered: false));

            Assert.IsTrue(PersistentIdEvictionPolicy.ShouldRestoreLoadedOwner(
                ownerVesselStillPresent: true, idAlreadyRegistered: false));

            Assert.IsFalse(PersistentIdEvictionPolicy.ShouldRestoreLoadedOwner(
                ownerVesselStillPresent: true, idAlreadyRegistered: true));
        }

        [TestMethod]
        public void DanglingSweepSelectsOnlyAbsentOwnersOfThisVessel()
        {
            var vesselId = new System.Guid("10000000-0000-0000-0000-000000000001");
            var otherVesselId = new System.Guid("20000000-0000-0000-0000-000000000002");

            var previousCopy = new FakeOwner(vesselId, presentInGameState: true);
            var partialFromThrowingCtor = new FakeOwner(vesselId, presentInGameState: false);
            var foreignOwner = new FakeOwner(otherVesselId, presentInGameState: false);

            var registry = new object[] { null, previousCopy, partialFromThrowingCtor, foreignOwner };

            var selected = new List<object>();
            foreach (var owner in registry)
            {
                if (PersistentIdEvictionPolicy.IsDanglingVesselRegistration(
                        owner, vesselId, o => ((FakeOwner)o).VesselId, o => ((FakeOwner)o).PresentInGameState))
                    selected.Add(owner);
            }

            CollectionAssert.AreEqual(new[] { partialFromThrowingCtor }, selected);
        }

        private sealed class FakeOwner
        {
            public readonly System.Guid VesselId;
            public readonly bool PresentInGameState;

            public FakeOwner(System.Guid vesselId, bool presentInGameState)
            {
                VesselId = vesselId;
                PresentInGameState = presentInGameState;
            }
        }
    }
}
