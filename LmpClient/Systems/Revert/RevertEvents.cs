using LmpClient.Base;
using System;

namespace LmpClient.Systems.Revert
{
    public class RevertEvents : SubSystem<RevertSystem>
    {
        public void FlightReady()
        {
            System.StartingVesselId = FlightGlobals.ActiveVessel ? FlightGlobals.ActiveVessel.id : Guid.Empty;
        }
    }
}
