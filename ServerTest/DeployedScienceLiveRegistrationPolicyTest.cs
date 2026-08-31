using LmpClient.Systems.Scenario;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerTest
{
    [TestClass]
    public class DeployedScienceLiveRegistrationPolicyTest
    {
        [TestMethod]
        public void SettledPartMissingFromClusterRegisters()
        {
            Assert.IsTrue(DeployedScienceLiveRegistrationPolicy.ShouldRegister(
                deployedOnGround: true, beingDeployed: false, controlUnitIdAssigned: true,
                clusterFound: true, clusterContainsPart: false,
                isExperiment: false, clusterHasSameExperiment: false));
        }

        [TestMethod]
        public void PartAlreadyInClusterIsSkipped()
        {
            Assert.IsFalse(DeployedScienceLiveRegistrationPolicy.ShouldRegister(
                deployedOnGround: true, beingDeployed: false, controlUnitIdAssigned: true,
                clusterFound: true, clusterContainsPart: true,
                isExperiment: false, clusterHasSameExperiment: false));
        }

        [TestMethod]
        public void TransientDeploymentStatesAreSkipped()
        {
            Assert.IsFalse(DeployedScienceLiveRegistrationPolicy.ShouldRegister(
                deployedOnGround: false, beingDeployed: true, controlUnitIdAssigned: true,
                clusterFound: true, clusterContainsPart: false,
                isExperiment: false, clusterHasSameExperiment: false));
            Assert.IsFalse(DeployedScienceLiveRegistrationPolicy.ShouldRegister(
                deployedOnGround: true, beingDeployed: true, controlUnitIdAssigned: true,
                clusterFound: true, clusterContainsPart: false,
                isExperiment: false, clusterHasSameExperiment: false));
        }

        [TestMethod]
        public void UnassignedOrClusterlessPartsAreLeftToStockScan()
        {
            Assert.IsFalse(DeployedScienceLiveRegistrationPolicy.ShouldRegister(
                deployedOnGround: true, beingDeployed: false, controlUnitIdAssigned: false,
                clusterFound: true, clusterContainsPart: false,
                isExperiment: false, clusterHasSameExperiment: false));
            Assert.IsFalse(DeployedScienceLiveRegistrationPolicy.ShouldRegister(
                deployedOnGround: true, beingDeployed: false, controlUnitIdAssigned: true,
                clusterFound: false, clusterContainsPart: false,
                isExperiment: false, clusterHasSameExperiment: false));
        }

        [TestMethod]
        public void DuplicateExperimentIsRefusedLikeStock()
        {
            Assert.IsFalse(DeployedScienceLiveRegistrationPolicy.ShouldRegister(
                deployedOnGround: true, beingDeployed: false, controlUnitIdAssigned: true,
                clusterFound: true, clusterContainsPart: false,
                isExperiment: true, clusterHasSameExperiment: true));
            Assert.IsTrue(DeployedScienceLiveRegistrationPolicy.ShouldRegister(
                deployedOnGround: true, beingDeployed: false, controlUnitIdAssigned: true,
                clusterFound: true, clusterContainsPart: false,
                isExperiment: true, clusterHasSameExperiment: false));
        }
    }
}
