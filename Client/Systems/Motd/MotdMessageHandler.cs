using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Chat;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Motd
{
    public class MotdMessageHandler : SubSystem<MotdSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is MotdReplyMsgData msgData)) return;

            if (!string.IsNullOrEmpty(msgData.MessageOfTheDay))
            {
                ChatSystem.Singleton.PrintToChat(msgData.MessageOfTheDay);
                LunaScreenMsg.PostScreenMessage(msgData.MessageOfTheDay, 30f, ScreenMessageStyle.UPPER_CENTER);
            }
        }
    }
}
