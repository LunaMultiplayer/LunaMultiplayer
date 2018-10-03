using System.Reflection;
using KSP.UI.Screens;
using LmpClient.Base;
using LmpCommon.Locks;

namespace LmpClient.Systems.KscScene
{
    public class KscSceneEvents: SubSystem<KscSceneSystem>
    {
        private static readonly MethodInfo ClearVesselMarkers = typeof(KSCVesselMarkers).GetMethod("ClearVesselMarkers", BindingFlags.NonPublic | BindingFlags.Instance);

        public void OnLockAcquire(LockDefinition lockdefinition)
        {
            KSCVesselMarkers.fetch?.RefreshMarkers();
        }

        public void OnLockRelease(LockDefinition lockdefinition)
        {
            KSCVesselMarkers.fetch?.RefreshMarkers();
        }

        /// <summary>
        /// Sometimes the vessel markers stay there with corrupt values so here we force clearing them
        /// </summary>
        public void OnSceneRequested(GameScenes requestedScene)
        {
            if (requestedScene > GameScenes.SPACECENTER && KSCVesselMarkers.fetch != null)
            {
                ClearVesselMarkers?.Invoke(KSCVesselMarkers.fetch, null);
            }
        }

        /// <summary>
        /// Sometimes the vessel markers stay there with corrupt values so here we force clearing them
        /// </summary>
        public void LevelLoaded(GameScenes data)
        {
            if (data == GameScenes.SPACECENTER)
                KSCVesselMarkers.fetch?.RefreshMarkers();
        }
    }
}
