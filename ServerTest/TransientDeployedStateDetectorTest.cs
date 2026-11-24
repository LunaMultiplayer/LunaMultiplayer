using Microsoft.VisualStudio.TestTools.UnitTesting;
using LmpClient.VesselUtilities;

namespace ServerTest
{
    [TestClass]
    public class TransientDeployedStateDetectorTest
    {
        [TestMethod]
        public void GroundModuleFamilyIsRecognized()
        {
            Assert.IsTrue(TransientDeployedStateDetector.IsGroundModule("ModuleGroundPart"));
            Assert.IsTrue(TransientDeployedStateDetector.IsGroundModule("ModuleGroundSciencePart"));
            Assert.IsTrue(TransientDeployedStateDetector.IsGroundModule("ModuleGroundExperiment"));
            Assert.IsTrue(TransientDeployedStateDetector.IsGroundModule("ModuleGroundExpControl"));
            Assert.IsTrue(TransientDeployedStateDetector.IsGroundModule("ModuleGroundCommsPart"));

            Assert.IsFalse(TransientDeployedStateDetector.IsGroundModule("ModuleCommand"));
            Assert.IsFalse(TransientDeployedStateDetector.IsGroundModule("ModuleReactionWheel"));
            Assert.IsFalse(TransientDeployedStateDetector.IsGroundModule(null));
            Assert.IsFalse(TransientDeployedStateDetector.IsGroundModule(string.Empty));
        }

        [TestMethod]
        public void TransientDeploymentIsDetected()
        {
            Assert.IsTrue(TransientDeployedStateDetector.IsTransientDeployedModule("ModuleGroundExperiment", true, false, false));
            Assert.IsTrue(TransientDeployedStateDetector.IsTransientDeployedModule("ModuleGroundExperiment", true, false, true));
            Assert.IsTrue(TransientDeployedStateDetector.IsTransientDeployedModule("ModuleGroundExperiment", true, true, false));
        }

        [TestMethod]
        public void SettledModuleIsNotTransient()
        {
            Assert.IsFalse(TransientDeployedStateDetector.IsTransientDeployedModule("ModuleGroundExperiment", false, true, true));
        }

        [TestMethod]
        public void BeingDeployedWithSettledEvidenceIsNotTransient()
        {
            Assert.IsFalse(TransientDeployedStateDetector.IsTransientDeployedModule("ModuleGroundExperiment", true, true, true));
        }

        [TestMethod]
        public void MissingSettlementEvidenceIsNotTransient()
        {
            Assert.IsFalse(TransientDeployedStateDetector.IsTransientDeployedModule("ModuleGroundExperiment", true, null, null));
        }

        [TestMethod]
        public void NonGroundModuleIsNotTransient()
        {
            Assert.IsFalse(TransientDeployedStateDetector.IsTransientDeployedModule("ModuleCommand", true, false, false));
            Assert.IsFalse(TransientDeployedStateDetector.IsTransientDeployedModule(null, true, false, false));
        }

        [TestMethod]
        public void NullBeingDeployedIsNotTransient()
        {
            Assert.IsFalse(TransientDeployedStateDetector.IsTransientDeployedModule("ModuleGroundExperiment", null, false, false));
        }
    }
}
