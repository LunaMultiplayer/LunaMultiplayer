using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselStore;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    public class VesselPartModuleSyncMessageSender : SubSystem<VesselPartModuleSyncSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPartSyncMsg(Guid vesselId, uint partFlightId, string moduleName, string baseModuleName, string fieldName, string value)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPartSyncMsgData>();
            msgData.VesselId = vesselId;
            msgData.PartFlightId = partFlightId;
            msgData.ModuleName = moduleName;
            msgData.BaseModuleName = baseModuleName;
            msgData.FieldName = fieldName;
            msgData.Value = value;

            VesselsProtoStore.UpdateVesselProtoPartModules(msgData);
            SendMessage(msgData);
        }
    }
}
