using LunaClient.Base;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using System;

namespace LunaClient.Systems.VesselPartSyncUiFieldSys
{
    public class VesselPartSyncUiFieldQueue : CachedConcurrentQueue<VesselPartSyncUiField, VesselPartSyncUiFieldMsgData>
    {
        protected override void AssignFromMessage(VesselPartSyncUiField value, VesselPartSyncUiFieldMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;
            value.VesselPersistentId = msgData.VesselPersistentId;

            value.PartFlightId = msgData.PartFlightId;
            value.PartPersistentId = msgData.PartPersistentId;
            value.ModuleName = msgData.ModuleName.Clone() as string;
            value.FieldName = msgData.FieldName.Clone() as string;

            value.FieldType = msgData.FieldType;

            switch (value.FieldType)
            {
                case PartSyncFieldType.Boolean:
                    value.BoolValue = msgData.BoolValue;
                    break;
                case PartSyncFieldType.Integer:
                    value.IntValue = msgData.IntValue;
                    break;
                case PartSyncFieldType.Float:
                    value.FloatValue = msgData.FloatValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
