using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Chat;
using LunaClient.Systems.Status;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaCommon.Message.Data.PlayerConnection;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.PlayerConnection
{
    public class PlayerConnectionMessageHandler : SubSystem<PlayerConnectionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is PlayerConnectionBaseMsgData msgData)) return;

            var playerName = msgData.PlayerName;
            switch (msgData.PlayerConnectionMessageType)
            {
                case PlayerConnectionMessageType.Join:
                    ChatSystem.Singleton.PrintToChat($"{playerName} has joined the server");
                    CommonUtil.Reserve20Mb();
                    break;
                case PlayerConnectionMessageType.Leave:
                    WarpSystem.Singleton.RemovePlayer(playerName);
                    StatusSystem.Singleton.RemovePlayer(playerName);
                    ChatSystem.Singleton.PrintToChat($"{playerName} has left the server");
                    break;
            }
        }
    }
}
