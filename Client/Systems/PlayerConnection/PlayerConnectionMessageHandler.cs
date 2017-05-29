using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Chat;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.Warp;
using LunaCommon.Message.Data.PlayerConnection;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

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
                case PlayerConnectionMessageType.Join:
                    SystemsContainer.Get<ChatSystem>().Queuer.QueueChannelMessage(SettingsSystem.ServerSettings.ConsoleIdentifier, "", $"{playerName} has joined the server");
                    break;
                case PlayerConnectionMessageType.Leave:
                    SystemsContainer.Get<WarpSystem>().RemovePlayer(playerName);
                    SystemsContainer.Get<StatusSystem>().RemovePlayer(playerName);
                    SystemsContainer.Get<ChatSystem>().Queuer.QueueRemovePlayer(playerName);
                    SystemsContainer.Get<ChatSystem>().Queuer.QueueChannelMessage(SettingsSystem.ServerSettings.ConsoleIdentifier, "", $"{playerName} has left the server");
                    break;
            }
        }
    }
}