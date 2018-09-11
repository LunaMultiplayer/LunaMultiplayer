using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.ModuleStore;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselPartModuleFieldSyncSys
{
    public class VesselPartModuleFieldSyncMessageSender : SubSystem<VesselPartModuleFieldSyncSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPartSyncMsg(Guid vesselId, uint partFlightId, string moduleName, string baseModuleName, string fieldName, string value)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPartFieldSyncMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vesselId;
            msgData.PartFlightId = partFlightId;
            msgData.ModuleName = moduleName;
            msgData.BaseModuleName = baseModuleName;
            msgData.FieldName = fieldName;
            msgData.Value = value;

            SendMessage(msgData);
        }
    }
}
