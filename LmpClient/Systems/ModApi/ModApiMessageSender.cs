using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.ModApi
{
    public class ModApiMessageSender : SubSystem<ModApiSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ModCliMsg>(msg)));
        }
    }
}