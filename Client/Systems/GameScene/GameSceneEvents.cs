using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselStateSys;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
using LunaCommon.Locks;
using System.Reflection;

namespace LunaClient.Systems.GameScene
{
    public class GameSceneEvents : SubSystem<GameSceneSystem>
    {
        private static readonly MethodInfo ClearVesselMarkers = typeof(KSCVesselMarkers).GetMethod("ClearVesselMarkers", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Called when the scene changes
        /// </summary>
        public void OnSceneChanged(GameScenes data)
        {
            if (data == GameScenes.FLIGHT) return;

            //Always release the Update/UnloadedUpdate lock and the spectate lock
            SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.Update);
            SystemsContainer.Get<LockSystem>().ReleasePlayerLocks(LockType.UnloadedUpdate);
            SystemsContainer.Get<VesselLockSystem>().StopSpectating();

            //We are going to another screen so clear up the systems
            SystemsContainer.Get<VesselRemoveSystem>().ClearSystem();
            SystemsContainer.Get<VesselFlightStateSystem>().ClearSystem();
            SystemsContainer.Get<VesselStateSystem>().ClearSystem();

            switch (data)
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

            if (data == GameScenes.SPACECENTER)
            {
                //If we are going to space center clear all the vessels.
                //This will avoid all the headaches of recovering vessels and so on with the KSCVesselMarkers.
                //Those markers appear on the KSC when you return from flight but they are NEVER updated
                //So if a vessel from another player was in the launchpad and you return to the KSC, even 
                //if that player goes to orbit, you will see the marker on the launchpad. This means that you 
                //won't be able to launch without recovering it and if that player release the control lock,
                //you will be recovering a valid vessel that is already in space.

                if(KSCVesselMarkers.fetch != null)
                    ClearVesselMarkers?.Invoke(KSCVesselMarkers.fetch, null);
                DelayedClearVessels();
            }
        }

        /// <summary>
        /// This coroutine removes the vessels when switching to the KSC. We delay the removal of the vessels so 
        /// in case we recover a vessel while in flight we correctly recover the crew, funds etc
        /// </summary>
        private static void DelayedClearVessels()
        {
            CoroutineUtil.StartDelayedRoutine(() =>
            {
                FlightGlobals.Vessels.Clear();
                HighLogic.CurrentGame?.flightState?.protoVessels?.Clear();
                KSCVesselMarkers.fetch?.RefreshMarkers();
            }, 3);
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
