using System;
using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.Chat;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.Chat
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
