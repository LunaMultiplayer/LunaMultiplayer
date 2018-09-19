using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Motd;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.Motd
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
