using LunaClient.Base;

namespace LunaClient.Systems.VesselFairingsSys
{
    public class VesselFairingEvents : SubSystem<VesselFairingsSystem>
    {
        public void FairingsDeployed(Part part)
        {
            LunaLog.Log($"Detected fairings deployed! Part FlightID: {part.flightID}");
            System.MessageSender.SendVesselFairingDeployed(FlightGlobals.ActiveVessel, part.flightID);
        }
    }
}
