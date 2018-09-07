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

            //Release all vessel locks as we are switching to a NEW vessel.
            LockSystem.Singleton.ReleasePlayerLocks(LockType.Update);
            LockSystem.Singleton.ReleasePlayerLocks(LockType.UnloadedUpdate);
            LockSystem.Singleton.ReleasePlayerLocks(LockType.Control);
            LockSystem.Singleton.ReleasePlayerLocks(LockType.Kerbal);

            if (LockSystem.LockQuery.ControlLockExists(vessel.id) && !LockSystem.LockQuery.ControlLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
            {
                //We switched to a vessel that is controlled by another player so start spectating
                System.StartSpectating(vessel.id);
            }
            else
            {
                LockSystem.Singleton.AcquireControlLock(vessel.id);
                LockSystem.Singleton.AcquireUpdateLock(vessel.id);
                LockSystem.Singleton.AcquireUnloadedUpdateLock(vessel.id);
                LockSystem.Singleton.AcquireKerbalLock(vessel);
            }
        }

        /// <summary>
        /// Event called when switching scene and before reaching the other scene
        /// </summary>
        internal void OnSceneRequested(GameScenes requestedScene)
        {
            if (requestedScene != GameScenes.FLIGHT)
            {
                InputLockManager.RemoveControlLock(VesselLockSystem.SpectateLock);
                VesselLockSystem.Singleton.StopSpectating();
            }
        }

        /// <summary>
        /// Be extra sure that we remove the spectate lock
        /// </summary>
        public void LevelLoaded(GameScenes data)
        {
            if (data != GameScenes.FLIGHT)
            {
                InputLockManager.RemoveControlLock(VesselLockSystem.SpectateLock);
                VesselLockSystem.Singleton.StopSpectating();
            }
        }
    }
}
