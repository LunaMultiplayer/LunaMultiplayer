using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Facility;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.Facility
{
    public class FacilityMessageSender : SubSystem<FacilitySystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<FacilityCliMsg>(msg)));
        }
        
        public void SendFacilityCollapseMsg(string objectId)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<FacilityCollapseMsgData>();
            msgData.ObjectId = objectId;

            SendMessage(msgData);
        }

        public void SendFacilityRepairMsg(string objectId)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<FacilityRepairMsgData>();
            msgData.ObjectId = objectId;

            SendMessage(msgData);
        }
    }
}
