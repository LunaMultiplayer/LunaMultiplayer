using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSyncer;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselPartSyncCallSys
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
            msgData.PartFlightId = part.flightID;
            msgData.ModuleName = moduleName;
            msgData.MethodName = methodName;

            SendMessage(msgData);
        }
    }
}
