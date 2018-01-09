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
            //Safety check
            if (vessel == null) return;

            //In case we are reloading our current own vessel we DON'T want to release our locks
            //As that would mean that an spectator could get the control of our vessel while we are reloading it.
            //Therefore we just ignore this whole thing to avoid releasing our locks.
            //Reloading our own current vessel is a bad practice so this case should not happen anyway...
            if (LockSystem.LockQuery.GetControlLockOwner(vessel.id) == SettingsSystem.CurrentSettings.PlayerName)
                return;

            //Release all update locks as we are switching to a NEW vessel.
            SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.Update);
            SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.UnloadedUpdate);
            if (SettingsSystem.ServerSettings.DropControlOnVesselSwitching)
            {
                //Drop all the control locks if we are switching and the above setting is on
                SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.Control);
            }

            if (LockSystem.LockQuery.ControlLockExists(vessel.id) && !LockSystem.LockQuery.ControlLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
            {
                //We switched to a vessel that is controlled by another player so start spectating
                System.StartSpectating(vessel.id);
            }
        }
    }
}
