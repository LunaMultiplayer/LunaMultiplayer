using LmpClient.Base;
using System;

namespace LmpClient.Systems.Revert
{
    public class RevertEvents : SubSystem<RevertSystem>
    {
        private static bool _revertingToLaunch = false;

        public void OnVesselChange(Vessel data)
        {
            if (_revertingToLaunch)
            {
                _revertingToLaunch = false;
                return;
            }

            System.StartingVesselId = Guid.Empty;
        }
        
        public void VesselAssembled(Vessel vessel, ShipConstruct construct)
        {
            System.StartingVesselId = vessel.id;
        }

        public void OnRevertToLaunch()
        {
            _revertingToLaunch = true;
            if (FlightGlobals.ActiveVessel)
                System.StartingVesselId = FlightGlobals.ActiveVessel.id;
        }

        public void GameSceneLoadRequested(GameScenes data)
        {
            if (data != GameScenes.FLIGHT && _revertingToLaunch)
                _revertingToLaunch = false;
        }
    }
}
