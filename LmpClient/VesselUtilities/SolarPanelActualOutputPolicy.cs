namespace LmpClient.VesselUtilities
{
    public static class SolarPanelActualOutputPolicy
    {
        public static int? ComputeTarget(
            bool moduleBehaviourEnabled,
            bool hasSecondaryTransform,
            bool hasTrackingBody,
            bool timeWarpRateIsOne,
            bool hasLineOfSight,
            bool deployedOnGround,
            bool isEnabled,
            int potentialOutput)
        {
            if (moduleBehaviourEnabled && hasSecondaryTransform && hasTrackingBody && timeWarpRateIsOne)
                return hasLineOfSight ? potentialOutput : 0;

            //Stock freezes the last actual value while timewarping / no tracking body — don't overwrite.
            if (deployedOnGround && isEnabled && moduleBehaviourEnabled)
                return null;

            return 0;
        }
    }
}
