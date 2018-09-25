using LmpClient.Base;

namespace LmpClient.Systems.VesselFairingsSys
{
    public class VesselFairingEvents : SubSystem<VesselFairingsSystem>
    {
        public void FairingsDeployed(Part part)
        {
            LunaLog.Log($"Detected fairings deployed! Part: {part.partName}");
            System.MessageSender.SendVesselFairingDeployed(FlightGlobals.ActiveVessel, part.flightID);
        }
    }
}
