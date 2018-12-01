using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.VesselCoupleSys
{
    public class VesselCoupleMessageSender : SubSystem<VesselCoupleSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselCouple(Vessel vessel, uint partFlightId, Guid coupledVesselId, uint coupledPartFlightId, bool grapple)
        {
            if (vessel == null) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselCoupleMsgData>();
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.PartFlightId = partFlightId;
            msgData.CoupledVesselId = coupledVesselId;
            msgData.CoupledPartFlightId = coupledPartFlightId;
            msgData.SubspaceId = WarpSystem.Singleton.CurrentSubspace;
            msgData.Grapple = grapple;

            SendMessage(msgData);
        }
    }
}
