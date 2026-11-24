using Microsoft.VisualStudio.TestTools.UnitTesting;
using LmpClient.VesselUtilities;

namespace ServerTest
{
    [TestClass]
    public class SolarPanelActualOutputPolicyTest
    {
        private const int Potential = 5;

        [TestMethod]
        public void DayWithLineOfSightProducesPotential()
        {
            Assert.AreEqual(Potential, SolarPanelActualOutputPolicy.ComputeTarget(
                moduleBehaviourEnabled: true, hasSecondaryTransform: true, hasTrackingBody: true,
                timeWarpRateIsOne: true, hasLineOfSight: true,
                deployedOnGround: true, isEnabled: true, potentialOutput: Potential));
        }

        [TestMethod]
        public void NightWithoutLineOfSightProducesZero()
        {
            Assert.AreEqual(0, SolarPanelActualOutputPolicy.ComputeTarget(
                moduleBehaviourEnabled: true, hasSecondaryTransform: true, hasTrackingBody: true,
                timeWarpRateIsOne: true, hasLineOfSight: false,
                deployedOnGround: true, isEnabled: true, potentialOutput: Potential));
        }

        [TestMethod]
        public void TimeWarpNotOneFreezesLastValue()
        {
            Assert.IsNull(SolarPanelActualOutputPolicy.ComputeTarget(
                moduleBehaviourEnabled: true, hasSecondaryTransform: true, hasTrackingBody: true,
                timeWarpRateIsOne: false, hasLineOfSight: true,
                deployedOnGround: true, isEnabled: true, potentialOutput: Potential));
        }

        [TestMethod]
        public void MissingTrackingBodyFreezesLastValue()
        {
            Assert.IsNull(SolarPanelActualOutputPolicy.ComputeTarget(
                moduleBehaviourEnabled: true, hasSecondaryTransform: false, hasTrackingBody: false,
                timeWarpRateIsOne: true, hasLineOfSight: false,
                deployedOnGround: true, isEnabled: true, potentialOutput: Potential));
        }

        [TestMethod]
        public void DisabledModuleProducesZero()
        {
            Assert.AreEqual(0, SolarPanelActualOutputPolicy.ComputeTarget(
                moduleBehaviourEnabled: false, hasSecondaryTransform: true, hasTrackingBody: true,
                timeWarpRateIsOne: true, hasLineOfSight: true,
                deployedOnGround: true, isEnabled: true, potentialOutput: Potential));
        }

        [TestMethod]
        public void NotYetDeployedWithoutTrackingProducesZero()
        {
            Assert.AreEqual(0, SolarPanelActualOutputPolicy.ComputeTarget(
                moduleBehaviourEnabled: true, hasSecondaryTransform: false, hasTrackingBody: false,
                timeWarpRateIsOne: false, hasLineOfSight: false,
                deployedOnGround: false, isEnabled: false, potentialOutput: Potential));
        }

        [TestMethod]
        public void DeployedButIsEnabledFalseProducesZero()
        {
            Assert.AreEqual(0, SolarPanelActualOutputPolicy.ComputeTarget(
                moduleBehaviourEnabled: true, hasSecondaryTransform: false, hasTrackingBody: false,
                timeWarpRateIsOne: false, hasLineOfSight: false,
                deployedOnGround: true, isEnabled: false, potentialOutput: Potential));
        }

        [TestMethod]
        public void ZeroPotentialDayStaysZero()
        {
            Assert.AreEqual(0, SolarPanelActualOutputPolicy.ComputeTarget(
                moduleBehaviourEnabled: true, hasSecondaryTransform: true, hasTrackingBody: true,
                timeWarpRateIsOne: true, hasLineOfSight: true,
                deployedOnGround: true, isEnabled: true, potentialOutput: 0));
        }
    }
}
