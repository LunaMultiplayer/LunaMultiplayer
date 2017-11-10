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
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is PlayerConnectionBaseMsgData msgData)) return;

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