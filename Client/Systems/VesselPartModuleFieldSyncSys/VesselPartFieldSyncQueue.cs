using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;

namespace LunaClient.Systems.VesselPartModuleFieldSyncSys
{
    public class VesselPartFieldSyncQueue : CachedConcurrentQueue<VesselPartFieldSync, VesselPartFieldSyncMsgData>
    {
        protected override void AssignFromMessage(VesselPartFieldSync value, VesselPartFieldSyncMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;

            value.PartFlightId = msgData.PartFlightId;
            value.ModuleName = msgData.ModuleName.Clone() as string;
            value.BaseModuleName = msgData.BaseModuleName.Clone() as string;
            value.FieldName = msgData.FieldName.Clone() as string;
            value.Value = msgData.Value.Clone() as string;
        }
    }
}
