using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Lock;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.Lock
{
    public class LockMessageSender : SubSystem<LockSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<LockCliMsg>(msg)));
        }
        
        public void SendLocksRequest()
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<LockCliMsg, LockListRequestMsgData>()));
        }
    }
}