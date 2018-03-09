using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselStore;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselEvaSys
{
    public class VesselEvaMessageSender : SubSystem<VesselEvaSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendEvaData(Vessel vessel, string newState, string eventToRun, float lastBoundStep = float.NaN)
        {
            if (vessel == null) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselEvaMsgData>();
            msgData.VesselId = vessel.id;
            msgData.NewState = newState;
            msgData.EventToRun = eventToRun;
            msgData.LastBoundStep = lastBoundStep;

            SendMessage(msgData);

            VesselsProtoStore.UpdateVesselProtoEvaFsm(msgData);
        }
    }
}
