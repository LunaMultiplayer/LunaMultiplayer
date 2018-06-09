using LunaClient.Base;
using LunaCommon.Locks;

namespace LunaClient.Systems.VesselPositionSys
{
    public class PositionEvents : SubSystem<VesselPositionSystem>
    {
        public void OnLockAcquire(LockDefinition data)
        {
            switch (data.Type)
            {
                case LockType.UnloadedUpdate:
                case LockType.Update:
                case LockType.Control:
                    System.RemoveVessel(data.VesselId);
                    break;
            }
        }
    }
}
