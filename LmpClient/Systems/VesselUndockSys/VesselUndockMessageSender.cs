using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.VesselUndockSys
{
    public class VesselUndockMessageSender : SubSystem<VesselUndockSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselUndock(Vessel vessel, uint partFlightId, DockedVesselInfo dockedInfo, Guid newVesselId)
        {
            if (vessel == null) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselUndockMsgData>();
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.PartFlightId = partFlightId;
            msgData.NewVesselId = newVesselId;
            msgData.DockedInfoName = dockedInfo.name;
            msgData.DockedInfoRootPartUId = dockedInfo.rootPartUId;
            msgData.DockedInfoVesselType = (int)dockedInfo.vesselType;

            SendMessage(msgData);
        }
    }
}
