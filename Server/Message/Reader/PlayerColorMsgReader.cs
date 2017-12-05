using System.Collections.Generic;
using System.Linq;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Context;
using Server.Message.Reader.Base;
using Server.Server;

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
                    if (data.PlayerName != client.PlayerName) return;

                    client.PlayerColor = data.Color;
                    MessageQueuer.RelayMessage<PlayerColorSrvMsg>(client, data);
                    break;
            }
        }

        private static void SendAllPlayerColors(ClientStructure client)
        {
            var sendColors = new Dictionary<string, string>();

            foreach (var otherClient in ClientRetriever.GetAuthenticatedClients().Where(c => !Equals(c, client) && c.PlayerColor != null))
                sendColors[otherClient.PlayerName] = otherClient.PlayerColor;

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<PlayerColorReplyMsgData>();
            msgData.PlayersColors = sendColors.ToArray();

            MessageQueuer.SendToClient<PlayerColorSrvMsg>(client, msgData);
        }
    }
}