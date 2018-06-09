using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Locks;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class FlightStateEvents : SubSystem<VesselFlightStateSystem>
    {
        public void OnVesselPack(Vessel vessel)
        {
            System.RemoveVessel(vessel);
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
            System.RemoveVessel(FlightGlobals.ActiveVessel);
        }
        
        /// <summary>
        /// Whenever we acquire a UnloadedUpdate/Update/Control lock of a vessel, remove it from the dictionaries
        /// </summary>
        public void OnLockAcquire(LockDefinition data)
        {
            if (data.PlayerName != SettingsSystem.CurrentSettings.PlayerName)
                return;

            switch (data.Type)
            {
                case LockType.UnloadedUpdate:
                case LockType.Update:
                case LockType.Control:
                    System.RemoveVessel(data.VesselId);
                    break;
            }
        }
    }
}
