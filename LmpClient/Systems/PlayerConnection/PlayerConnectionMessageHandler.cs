using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.Chat;
using LmpClient.Systems.Status;
using LmpClient.Systems.Warp;
using LmpClient.Utilities;
using LmpCommon.Message.Data.PlayerConnection;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;

namespace LmpClient.Systems.PlayerConnection
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
