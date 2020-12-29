using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.Chat;
using LmpCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace LmpClient.Systems.Chat
{
    public class ChatMessageHandler : SubSystem<ChatSystem>, IMessageHandler
    {
        private StringBuilder sb = new StringBuilder();
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ChatMsgData msgData)) return;

            sb.Length = 0;
            sb.Append(msgData.From).Append(": ").Append(msgData.Text);
            System.NewChatMessages.Enqueue(new Tuple<string, string, string>(msgData.From, msgData.Text, sb.ToString()));
        }
    }
}
