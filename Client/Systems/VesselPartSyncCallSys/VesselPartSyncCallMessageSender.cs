using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselPartSyncCallSys
{
    public class VesselPartSyncCallMessageSender : SubSystem<VesselPartSyncCallSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPartSyncCallMsg(Guid vesselId, uint partFlightId, string moduleName, string methodName)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPartSyncCallMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vesselId;
            msgData.PartFlightId = partFlightId;
            msgData.ModuleName = moduleName;
            msgData.MethodName = methodName;

            SendMessage(msgData);
        }
    }
}
