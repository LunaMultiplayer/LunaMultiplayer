using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Chat;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.Warp;
using LunaCommon.Message.Data.PlayerConnection;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaClient.Systems.PlayerConnection
{
    public class PlayerConnectionMessageHandler : SubSystem<PlayerConnectionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as PlayerConnectionBaseMsgData;
            if (msgData == null) return;

            var playerName = msgData.PlayerName;
            switch (msgData.PlayerConnectionMessageType)
            {
                case PlayerConnectionMessageType.JOIN:
                    ChatSystem.Singleton.Queuer.QueueChannelMessage(SettingsSystem.ServerSettings.ConsoleIdentifier, "", playerName + " has joined the server");
                    break;
                case PlayerConnectionMessageType.LEAVE:
                    WarpSystem.Singleton.RemovePlayer(playerName);
                    StatusSystem.Singleton.RemovePlayer(playerName);
                    ChatSystem.Singleton.Queuer.QueueRemovePlayer(playerName);
                    ChatSystem.Singleton.Queuer.QueueChannelMessage(SettingsSystem.ServerSettings.ConsoleIdentifier, "", playerName + " has left the server");
                    break;
            }
        }
    }
}