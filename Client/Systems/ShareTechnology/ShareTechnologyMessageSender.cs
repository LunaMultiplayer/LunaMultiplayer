using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.ShareTechnology
{
    public class ShareTechnologyMessageSender : SubSystem<ShareTechnologySystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendTechnologyMessage(string techId)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressTechnologyMsgData>();
            msgData.TechId = techId;
            SendMessage(msgData);
        }
    }
}
