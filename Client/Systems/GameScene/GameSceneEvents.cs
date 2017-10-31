using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaCommon.Locks;

namespace LunaClient.Systems.GameScene
{
    public class GameSceneEvents : SubSystem<GameSceneSystem>
    {
        /// <summary>
        /// Called when the scene changes
        /// </summary>
        public void OnSceneChanged(GameScenes data)
        {
            if (data == GameScenes.FLIGHT) return;

            //Always release the update lock and the spectate lock
            SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.Update);
            SystemsContainer.Get<LockSystem>().ReleaseSpectatorLock();
            InputLockManager.RemoveControlLock(VesselLockSystem.SpectateLock);

            //We are going to another screen so clear up the systems
            SystemsContainer.Get<VesselProtoSystem>().ClearSystem();
            SystemsContainer.Get<VesselRemoveSystem>().ClearSystem();
            SystemsContainer.Get<VesselFlightStateSystem>().ClearSystem();

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
            SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.Control);
            VesselCommon.IsSpectating = false;
        }

        /// <summary>
        /// Release the asteroid spawn lock
        /// </summary>
        private static void ReleaseAsteroidLock()
        {
            SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.Asteroid);
        }
    }
}
