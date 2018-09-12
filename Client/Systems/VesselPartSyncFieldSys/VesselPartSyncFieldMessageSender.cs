using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselPartSyncFieldSys
{
    public class VesselPartSyncFieldMessageSender : SubSystem<VesselPartSyncFieldSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPartSyncFieldMsg(Guid vesselId, uint partFlightId, string moduleName, string field, string value)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPartSyncFieldMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vesselId;
            msgData.PartFlightId = partFlightId;
            msgData.ModuleName = moduleName;
            msgData.FieldName = field;
            msgData.Value = value;

            SendMessage(msgData);
        }
    }
}
