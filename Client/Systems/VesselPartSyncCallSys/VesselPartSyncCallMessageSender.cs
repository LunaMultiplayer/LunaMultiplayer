using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselPartSyncCallSys
{
    public class VesselPartSyncCallMessageSender : SubSystem<VesselPartSyncCallSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPartSyncCallMsg(Vessel vessel, Part part, string moduleName, string methodName)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPartSyncCallMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.VesselPersistentId = vessel.persistentId;
            msgData.PartFlightId = part.flightID;
            msgData.PartPersistentId = part.persistentId;
            msgData.ModuleName = moduleName;
            msgData.MethodName = methodName;

            SendMessage(msgData);
        }
    }
}
