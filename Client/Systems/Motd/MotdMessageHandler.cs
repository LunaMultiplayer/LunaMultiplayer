using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Chat;
using LunaClient.Systems.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Motd
{
    public class MotdMessageHandler : SubSystem<MotdSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as MotdReplyMsgData;

            if (!string.IsNullOrEmpty(msgData?.MessageOfTheDay))
            {
                NetworkSystem.DisplayMotd = true;
                NetworkSystem.ServerMotd = msgData.MessageOfTheDay;
                ChatSystem.Singleton.Queuer.QueueChannelMessage(SettingsSystem.ServerSettings.ConsoleIdentifier, "", msgData.MessageOfTheDay);
            }
        }
    }
}