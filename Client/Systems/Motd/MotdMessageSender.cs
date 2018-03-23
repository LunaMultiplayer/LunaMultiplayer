using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Motd
{
    public class MotdMessageSender : SubSystem<MotdSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<MotdCliMsg>(msg)));
        }

        public void SendMotdRequest()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<MotdRequestMsgData>();
            SendMessage(msgData);
        }
    }
}
