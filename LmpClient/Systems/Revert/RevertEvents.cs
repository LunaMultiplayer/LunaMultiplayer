using LmpClient.Base;
using System;

namespace LmpClient.Systems.Revert
{
    public class RevertEvents : SubSystem<RevertSystem>
    {
        public void OnVesselChange(Vessel data)
        {
            System.StartingVesselId = Guid.Empty;
        }
        
        public void VesselAssembled(Vessel vessel, ShipConstruct construct)
        {
            System.StartingVesselId = vessel.id;
        }
    }
}
