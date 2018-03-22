using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Chat
{
    public class ChatMessageHandler : SubSystem<ChatSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ChatMsgData msgData)) return;

            System.NewChatMessages.Enqueue(new Tuple<string, string>(msgData.From, msgData.Text));
        }
    }
}