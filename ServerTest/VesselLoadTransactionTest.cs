using LmpClient.VesselUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerTest
{
    [TestClass]
    public class VesselLoadTransactionTest
    {
        [TestMethod]
        public void HappyPath_CommitOnlyImmediatelyBeforeFinalSuccess()
        {
            var tx = new VesselLoadTransaction();

            Assert.AreEqual(VesselLoadTransaction.Resolution.None, tx.Resolve(VesselLoadTransaction.Trigger.FinalSuccess));
            Assert.AreEqual(VesselLoadTransaction.Phase.AwaitingLoad, tx.CurrentPhase);

            Assert.AreEqual(VesselLoadTransaction.Resolution.Continue, tx.Resolve(VesselLoadTransaction.Trigger.LoadSucceeded));

            Assert.AreEqual(VesselLoadTransaction.Phase.LoadSucceeded, tx.CurrentPhase);

            Assert.AreEqual(VesselLoadTransaction.Resolution.Commit, tx.Resolve(VesselLoadTransaction.Trigger.FinalSuccess));
            Assert.AreEqual(VesselLoadTransaction.Phase.Committed, tx.CurrentPhase);

            Assert.AreEqual(VesselLoadTransaction.Resolution.None, tx.Resolve(VesselLoadTransaction.Trigger.PostLoadFailure));
        }

        [TestMethod]
        public void ExplicitFailureAfterLoad_RoutesThroughCleanupNotBareRollback()
        {
            var tx = new VesselLoadTransaction();
            tx.Resolve(VesselLoadTransaction.Trigger.LoadSucceeded);

            Assert.AreEqual(VesselLoadTransaction.Resolution.RollbackAndCleanUp,
                tx.Resolve(VesselLoadTransaction.Trigger.PostLoadFailure));
            Assert.AreEqual(VesselLoadTransaction.Phase.Failed, tx.CurrentPhase);
        }

        [TestMethod]
        public void ExceptionDuringPostLoadProcessing_RoutesThroughCleanup()
        {
            var tx = new VesselLoadTransaction();
            Assert.AreEqual(VesselLoadTransaction.Resolution.RollbackAndCleanUp,
                tx.Resolve(VesselLoadTransaction.Trigger.PostLoadFailure));

            var tx2 = new VesselLoadTransaction();
            tx2.Resolve(VesselLoadTransaction.Trigger.LoadSucceeded);
            Assert.AreEqual(VesselLoadTransaction.Resolution.RollbackAndCleanUp,
                tx2.Resolve(VesselLoadTransaction.Trigger.PostLoadFailure));
        }

        [TestMethod]
        public void LoadRefNull_RoutesThroughCleanup()
        {
            var tx = new VesselLoadTransaction();
            Assert.AreEqual(VesselLoadTransaction.Resolution.RollbackAndCleanUp,
                tx.Resolve(VesselLoadTransaction.Trigger.LoadRefNull));
            Assert.AreEqual(VesselLoadTransaction.Phase.Failed, tx.CurrentPhase);

            var tx2 = new VesselLoadTransaction();
            tx2.Resolve(VesselLoadTransaction.Trigger.LoadSucceeded);
            Assert.AreEqual(VesselLoadTransaction.Resolution.None, tx2.Resolve(VesselLoadTransaction.Trigger.LoadRefNull));
        }

        [TestMethod]
        public void UnchangedEarlyOut_RollbackOnly_NoTeardown()
        {
            var tx = new VesselLoadTransaction();
            Assert.AreEqual(VesselLoadTransaction.Resolution.RollbackOnly,
                tx.Resolve(VesselLoadTransaction.Trigger.UnchangedEarlyOut));
            Assert.AreEqual(VesselLoadTransaction.Phase.Failed, tx.CurrentPhase);
        }

        [TestMethod]
        public void ValidationFailure_RollbackOnly_NoTeardown()
        {
            var tx = new VesselLoadTransaction();
            Assert.AreEqual(VesselLoadTransaction.Resolution.RollbackOnly,
                tx.Resolve(VesselLoadTransaction.Trigger.ValidationFailed));
            Assert.AreEqual(VesselLoadTransaction.Phase.Failed, tx.CurrentPhase);
        }

        [TestMethod]
        public void ProtoSwapResolution_SwapResolveAndTerminal()
        {
            var tx = new VesselLoadTransaction();
            Assert.AreEqual(VesselLoadTransaction.Resolution.SwapResolve,
                tx.Resolve(VesselLoadTransaction.Trigger.ProtoSwapResolved));
            Assert.AreEqual(VesselLoadTransaction.Phase.Committed, tx.CurrentPhase);

            Assert.AreEqual(VesselLoadTransaction.Resolution.None, tx.Resolve(VesselLoadTransaction.Trigger.PostLoadFailure));
            Assert.AreEqual(VesselLoadTransaction.Resolution.None, tx.Resolve(VesselLoadTransaction.Trigger.FinalSuccess));
        }

        [TestMethod]
        public void EarlyOutAfterLoadSucceeded_IsDriverBug_Refused()
        {
            var tx = new VesselLoadTransaction();
            tx.Resolve(VesselLoadTransaction.Trigger.LoadSucceeded);
            Assert.AreEqual(VesselLoadTransaction.Resolution.None,
                tx.Resolve(VesselLoadTransaction.Trigger.UnchangedEarlyOut));
            Assert.AreEqual(VesselLoadTransaction.Phase.LoadSucceeded, tx.CurrentPhase);
        }
    }
}
