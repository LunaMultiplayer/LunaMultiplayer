using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.VesselDecoupleSys
{
    public class VesselDecoupleMessageSender : SubSystem<VesselDecoupleSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselDecouple(Vessel vessel, uint partFlightId, float breakForce, Guid newVesselId)
        {
            if (vessel == null) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselDecoupleMsgData>();
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.PartFlightId = partFlightId;
            msgData.BreakForce = breakForce;
            msgData.NewVesselId = newVesselId;

            SendMessage(msgData);
        }
    }
}
