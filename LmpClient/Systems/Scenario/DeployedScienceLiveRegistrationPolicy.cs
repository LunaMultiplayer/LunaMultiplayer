namespace LmpClient.Systems.Scenario
{
    public static class DeployedScienceLiveRegistrationPolicy
    {
        public static bool ShouldRegister(
            bool deployedOnGround, bool beingDeployed, bool controlUnitIdAssigned,
            bool clusterFound, bool clusterContainsPart,
            bool isExperiment, bool clusterHasSameExperiment)
        {
            if (beingDeployed || !deployedOnGround) return false;
            if (!controlUnitIdAssigned || !clusterFound) return false;
            if (clusterContainsPart) return false;
            if (isExperiment && clusterHasSameExperiment) return false;
            return true;
        }
    }
}
