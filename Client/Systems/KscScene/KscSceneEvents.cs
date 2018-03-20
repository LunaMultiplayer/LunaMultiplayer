using KSP.UI.Screens;
using LunaClient.Base;
using LunaCommon.Locks;

namespace LunaClient.Systems.KscScene
{
    public class KscSceneEvents: SubSystem<KscSceneSystem>
    {
        public void OnLockAcquire(LockDefinition lockdefinition)
        {
            KSCVesselMarkers.fetch?.RefreshMarkers();
        }

        public void OnLockRelease(LockDefinition lockdefinition)
        {
            KSCVesselMarkers.fetch?.RefreshMarkers();
        }
    }
}
