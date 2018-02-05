using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Context;
using Server.Message.Reader.Base;
using Server.Server;
using System.Linq;

namespace Server.Message.Reader
{
    public class PlayerColorMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as PlayerColorBaseMsgData;
            switch (message?.PlayerColorMessageType)
            {
                case PlayerColorMessageType.Request:
                    SendAllPlayerColors(client);
                    break;
                case PlayerColorMessageType.Set:
                    var data = (PlayerColorSetMsgData)message;
                    if (data.PlayerColor.PlayerName != client.PlayerName) return;

                    client.PlayerColor = data.PlayerColor.Color;
                    MessageQueuer.RelayMessage<PlayerColorSrvMsg>(client, data);
                    break;
            }
        }

        private static void SendAllPlayerColors(ClientStructure client)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<PlayerColorReplyMsgData>();
            msgData.PlayersColors = ClientRetriever.GetAuthenticatedClients().Where(c => !Equals(c, client))
                .Select(c => new PlayerColor
                {
                    PlayerName = c.PlayerName,
                    Color = c.PlayerColor
                }).ToArray();
            msgData.PlayerColorsCount = msgData.PlayersColors.Length;

            MessageQueuer.SendToClient<PlayerColorSrvMsg>(client, msgData);
        }
    }
}