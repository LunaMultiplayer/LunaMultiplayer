using LunaClient.Base;
using LunaCommon.Locks;

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

        public void OnLockAcquire(LockDefinition data)
        {
            switch (data.Type)
            {
                case LockType.UnloadedUpdate:
                case LockType.Update:
                case LockType.Control:
                    System.RemoveVesselFromSystem(data.VesselId);
                    break;
            }
        }
    }
}
