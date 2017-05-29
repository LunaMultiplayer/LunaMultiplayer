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
            SystemsContainer.Get<LockSystem>().ReleaseLocksWithPrefix("update-");
            SystemsContainer.Get<LockSystem>().ReleaseSpectatorLock();
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
            SystemsContainer.Get<LockSystem>().ReleaseLocksWithPrefix("control-");
            SystemsContainer.Get<LockSystem>().ReleaseSpectatorLock();
            VesselCommon.IsSpectating = false;
        }

        /// <summary>
        /// Release the asteroid spawn lock
        /// </summary>
        private static void ReleaseAsteroidLock()
        {
            SystemsContainer.Get<LockSystem>().ReleaseLocksWithPrefix("asteroid");
        }

        /// <summary>
        /// This event is called after a vessel has changed. Also called when starting a flight
        /// </summary>
        public void OnVesselChange(Vessel vessel)
        {
            //Release all update locks as we are switching to a new vessel. 
            //Anyway, we would still have the control lock of our old vessel
            SystemsContainer.Get<LockSystem>().ReleaseLocksWithPrefix("update-");

            if (SettingsSystem.ServerSettings.DropControlOnVesselSwitching)
            {
                //Drop all the control locks if we are switching and the above setting is on
                SystemsContainer.Get<LockSystem>().ReleaseLocksWithPrefix("control-");
            }

            if (vessel != null && (!SystemsContainer.Get<LockSystem>().LockExists($"control-{vessel.id}") || SystemsContainer.Get<LockSystem>().LockIsOurs($"control-{vessel.id}")))
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
