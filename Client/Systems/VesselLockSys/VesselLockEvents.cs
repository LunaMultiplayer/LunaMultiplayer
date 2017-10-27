using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Locks;

namespace LunaClient.Systems.VesselLockSys
{
    public class VesselLockEvents : SubSystem<VesselLockSystem>
    {
        /// <summary>
        /// This event is called after a vessel has changed. Also called when starting a flight
        /// </summary>
        public void OnVesselChange(Vessel vessel)
        {
            //Release all update locks as we are switching to a new vessel. 
            //Anyway, we would still have the control lock of our old vessel
            SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.Update);

            if (SettingsSystem.ServerSettings.DropControlOnVesselSwitching)
            {
                //Drop all the control locks if we are switching and the above setting is on
                SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.Control);
            }

            if (vessel != null && (!LockSystem.LockQuery.ControlLockExists(vessel.id) ||
                LockSystem.LockQuery.ControlLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName)))
            {
                //We managed to get the ship so set the update lock and in case we don't have the control lock aquire it.
                System.StopSpectatingAndGetControl(vessel, true);
            }
            else
            {
                System.StartSpectating();
            }
        }
    }
}
