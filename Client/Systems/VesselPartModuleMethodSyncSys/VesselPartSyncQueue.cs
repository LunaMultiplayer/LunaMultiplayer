using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;

namespace LunaClient.Systems.VesselPartModuleMethodSyncSys
{
    public class VesselPartSyncQueue : CachedConcurrentQueue<VesselPartMethodSync, VesselPartMethodSyncMsgData>
    {
        protected override void AssignFromMessage(VesselPartMethodSync value, VesselPartMethodSyncMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;

            value.PartFlightId = msgData.PartFlightId;
            value.ModuleName = msgData.ModuleName.Clone() as string;
            value.MethodName = msgData.MethodName.Clone() as string;

            value.FieldCount = msgData.FieldCount;
            if (value.FieldValues.Length < msgData.FieldCount)
                value.FieldValues = new FieldNameValue[msgData.FieldCount];

            for (var i = 0; i < msgData.FieldCount; i++)
            {
                if (value.FieldValues[i] == null)
                    value.FieldValues[i] = new FieldNameValue();

                value.FieldValues[i].Value = msgData.FieldValues[i].Value;
                value.FieldValues[i].FieldName = msgData.FieldValues[i].FieldName;
            }
        }
    }
}
