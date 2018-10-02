using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselFairingsSys
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
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.PartFlightId = partFlightId;

            SendMessage(msgData);
        }
    }
}
