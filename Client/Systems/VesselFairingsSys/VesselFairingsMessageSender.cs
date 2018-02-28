using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselStore;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselFairingsSys
{
    public class VesselFairingsMessageSender : SubSystem<VesselFairingsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselFairingDeployed(Vessel vessel, uint partFlightId)
        {
            if (vessel == null) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselFairingMsgData>();
            msgData.VesselId = vessel.id;
            msgData.PartFlightId = partFlightId;

            VesselsProtoStore.UpdateVesselProtoPartFairing(msgData);
            SendMessage(msgData);
        }
    }
}
