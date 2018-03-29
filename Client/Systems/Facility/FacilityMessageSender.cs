using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Facility;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Facility
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
