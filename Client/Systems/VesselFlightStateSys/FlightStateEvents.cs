using LunaClient.Base;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class FlightStateEvents : SubSystem<VesselFlightStateSystem>
    {
        public void OnVesselPack(Vessel vessel)
        {
            System.RemoveVesselFromSystem(vessel);
        }

        public void OnVesselUnpack(Vessel vessel)
        {
            System.AddVesselToSystem(vessel);
        }

        public void OnStartSpectating()
        {
            System.AddVesselToSystem(FlightGlobals.ActiveVessel);
        }

        public void OnFinishedSpectating()
        {
            System.RemoveVesselFromSystem(FlightGlobals.ActiveVessel);
        }
    }
}
