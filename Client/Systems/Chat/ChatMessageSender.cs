using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Chat
{
    public class ChatMessageSender : SubSystem<ChatSystem>, IMessageSender
    {
        public void SendMessage(IMessageData messageData)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ChatCliMsg>(messageData));
        }
    }
}