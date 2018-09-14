using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Enums;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Systems.VesselPartSyncFieldSys
{
    public class VesselPartSyncFieldMessageSender : SubSystem<VesselPartSyncFieldSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPartSyncFieldBoolMsg(Vessel vessel, Part part, string moduleName, string field, bool value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Boolean;
            msgData.BoolValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldIntMsg(Vessel vessel, Part part, string moduleName, string field, int value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Integer;
            msgData.IntValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldFloatMsg(Vessel vessel, Part part, string moduleName, string field, float value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Float;
            msgData.FloatValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldDoubleMsg(Vessel vessel, Part part, string moduleName, string field, double value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Double;
            msgData.DoubleValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldVectorMsg(Vessel vessel, Part part, string moduleName, string field, Vector3 value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Vector3;
            msgData.VectorValue[0] = value.x;
            msgData.VectorValue[1] = value.y;
            msgData.VectorValue[2] = value.z;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldQuaternionMsg(Vessel vessel, Part part, string moduleName, string field, Quaternion value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Quaternion;
            msgData.QuaternionValue[0] = value.x;
            msgData.QuaternionValue[1] = value.y;
            msgData.QuaternionValue[2] = value.z;
            msgData.QuaternionValue[3] = value.w;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldStringMsg(Vessel vessel, Part part, string moduleName, string field, string value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.String;
            msgData.StrValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncFieldObjectMsg(Vessel vessel, Part part, string moduleName, string field, object value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.String;
            msgData.StrValue = value.ToString();

            SendMessage(msgData);
        }

        private static VesselPartSyncFieldMsgData GetBaseMsg(Vessel vessel, Part part, string moduleName, string field)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPartSyncFieldMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.VesselPersistentId = vessel.persistentId;
            msgData.PartFlightId = part.flightID;
            msgData.PartPersistentId = part.persistentId;
            msgData.ModuleName = moduleName;
            msgData.FieldName = field;

            return msgData;
        }
    }
}
