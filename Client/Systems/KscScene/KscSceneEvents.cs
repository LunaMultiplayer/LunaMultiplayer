using LunaClient.Base;
using LunaCommon.Locks;

namespace LunaClient.Systems.KscScene
{
    public class KscSceneEvents: SubSystem<KscSceneSystem>
    {
        public void OnLockAcquire(LockDefinition lockdefinition)
        {
            //We don't trigger this directly as we are in another thread!
            System.TriggerMarkersRefresh = true;
        }

        public void OnLockRelease(LockDefinition lockdefinition)
        {            
            //We don't trigger this directly as we are in another thread!
            System.TriggerMarkersRefresh = true;
        }
    }
}
