using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.Chat;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Message.Data.Motd;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.Motd
{
    public class MotdMessageHandler : SubSystem<MotdSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is MotdReplyMsgData msgData)) return;

            if (!string.IsNullOrEmpty(msgData.MessageOfTheDay))
            {
                if (SettingsSystem.ServerSettings.PrintMotdInChat)
                    ChatSystem.Singleton.PrintToChat(msgData.MessageOfTheDay);

                LunaScreenMsg.PostScreenMessage(msgData.MessageOfTheDay, 30f, ScreenMessageStyle.UPPER_CENTER);
            }
        }
    }
}
