using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;

namespace LunaClient.Systems.VesselLockSys
{
    public class VesselLockEvents : SubSystem<VesselLockSystem>
    {
        /// <summary>
        /// This event is called after a game scene is changed.
        /// </summary>
        public void OnSceneChanged(GameScenes data)
        {
            if (data == GameScenes.FLIGHT) return;

            //Always release the update lock and the spectate lock
            LockSystem.Singleton.ReleaseLocksWithPrefix("update-");
            LockSystem.Singleton.ReleaseSpectatorLock();
            InputLockManager.RemoveControlLock(VesselLockSystem.SpectateLock);

            switch (data)
            {
                case GameScenes.MAINMENU:
                case GameScenes.SETTINGS:
                case GameScenes.CREDITS:
                    ReleaseAsteroidLock();
                    if (SettingsSystem.ServerSettings.DropControlOnExit)
                        ReleaseAllControlLocks();
                    break;
                case GameScenes.SPACECENTER:
                case GameScenes.EDITOR:
                case GameScenes.TRACKSTATION:
                case GameScenes.PSYSTEM:
                    if (SettingsSystem.ServerSettings.DropControlOnExitFlight)
                        ReleaseAllControlLocks();
                    break;
            }
        }

        /// <summary>
        /// Release the control locks
        /// </summary>
        private static void ReleaseAllControlLocks()
        {
            LockSystem.Singleton.ReleaseLocksWithPrefix("control-");
            LockSystem.Singleton.ReleaseSpectatorLock();
            VesselCommon.IsSpectating = false;
        }

        /// <summary>
        /// Release the asteroid spawn lock
        /// </summary>
        private static void ReleaseAsteroidLock()
        {
            LockSystem.Singleton.ReleaseLocksWithPrefix("asteroid");
        }

        /// <summary>
        /// This event is called after a vessel has changed. Also called when starting a flight
        /// </summary>
        public void OnVesselChange(Vessel vessel)
        {
            //Release all update locks as we are switching to a new vessel. 
            //Anyway, we would still have the control lock of our old vessel
            LockSystem.Singleton.ReleaseLocksWithPrefix("update-");

            if (SettingsSystem.ServerSettings.DropControlOnVesselSwitching)
            {
                //Drop all the control locks if we are switching and the above setting is on
                LockSystem.Singleton.ReleaseLocksWithPrefix("control-");
            }

            if (!LockSystem.Singleton.LockExists("control-" + vessel.id) || LockSystem.Singleton.LockIsOurs("control-" + vessel.id))
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
