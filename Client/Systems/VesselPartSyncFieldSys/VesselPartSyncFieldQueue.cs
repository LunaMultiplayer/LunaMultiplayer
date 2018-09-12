using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;

namespace LunaClient.Systems.VesselPartSyncFieldSys
{
    public class VesselPartSyncFieldQueue : CachedConcurrentQueue<VesselPartSyncField, VesselPartSyncFieldMsgData>
    {
        protected override void AssignFromMessage(VesselPartSyncField value, VesselPartSyncFieldMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;

            value.PartFlightId = msgData.PartFlightId;
            value.ModuleName = msgData.ModuleName.Clone() as string;
            value.FieldName = msgData.FieldName.Clone() as string;
            value.Value = msgData.Value.Clone() as string;
        }
    }
}
