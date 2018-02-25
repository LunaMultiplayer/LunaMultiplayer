using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Chat;
using LunaClient.Systems.SettingsSys;
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
                System.DisplayMotd = true;
                System.ServerMotd = msgData.MessageOfTheDay;
                ChatSystem.Singleton.Queuer.QueueChannelMessage(SettingsSystem.ServerSettings.ConsoleIdentifier, "", msgData.MessageOfTheDay);
            }
        }
    }
}