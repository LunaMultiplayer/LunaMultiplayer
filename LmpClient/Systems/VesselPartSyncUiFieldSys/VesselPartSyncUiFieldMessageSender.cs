using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSyncer;
using LmpCommon.Enums;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselPartSyncUiFieldSys
{
    public class VesselPartSyncUiFieldMessageSender : SubSystem<VesselPartSyncUiFieldSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPartSyncUiFieldBoolMsg(Vessel vessel, Part part, string moduleName, string field, bool value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Boolean;
            msgData.BoolValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncUiFieldIntMsg(Vessel vessel, Part part, string moduleName, string field, int value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Integer;
            msgData.IntValue = value;

            SendMessage(msgData);
        }

        public void SendVesselPartSyncUiFieldFloatMsg(Vessel vessel, Part part, string moduleName, string field, float value)
        {
            var msgData = GetBaseMsg(vessel, part, moduleName, field);
            msgData.FieldType = PartSyncFieldType.Float;
            msgData.FloatValue = value;

            SendMessage(msgData);
        }

        private static VesselPartSyncUiFieldMsgData GetBaseMsg(Vessel vessel, Part part, string moduleName, string field)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPartSyncUiFieldMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.PartFlightId = part.flightID;
            msgData.ModuleName = moduleName;
            msgData.FieldName = field;

            return msgData;
        }
    }
}
