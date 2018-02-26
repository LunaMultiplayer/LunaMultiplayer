using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselStateSys;
using LunaClient.VesselUtilities;
using LunaCommon.Locks;

namespace LunaClient.Systems.Lock
{
    public class LockEvents : SubSystem<LockSystem>
    {
        /// <summary>
        /// Triggered when requesting a scene change.
        /// </summary>
        public void OnSceneRequested(GameScenes requestedScene)
        {
            if (requestedScene == GameScenes.FLIGHT) return;

            //Always release the Update/UnloadedUpdate lock and the spectate lock
            System.ReleasePlayerLocks(LockType.Update);
            System.ReleasePlayerLocks(LockType.UnloadedUpdate);
            
            //We are going to another screen so clear up the systems
            VesselRemoveSystem.Singleton.ClearSystem();
            VesselFlightStateSystem.Singleton.ClearSystem();
            VesselStateSystem.Singleton.ClearSystem();

            switch (requestedScene)
            {
                case GameScenes.MAINMENU:
                case GameScenes.SETTINGS:
                case GameScenes.CREDITS:
                    ReleaseAsteroidLock();
                    if (SettingsSystem.ServerSettings.DropControlOnExit)
                        ReleaseAllControlLocks();
                    //When going to main menu activate LMP in case it was hidden
                    MainSystem.ToolbarShowGui = true;
                    break;
                case GameScenes.SPACECENTER:
                case GameScenes.EDITOR:
                case GameScenes.TRACKSTATION:
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
            System.ReleasePlayerLocks(LockType.Control);
            VesselCommon.IsSpectating = false;
        }

        /// <summary>
        /// Release the asteroid spawn lock
        /// </summary>
        private static void ReleaseAsteroidLock()
        {
            System.ReleasePlayerLocks(LockType.Asteroid);
        }
    }
}
