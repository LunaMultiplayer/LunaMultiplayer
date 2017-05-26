using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using System.Collections.Generic;
using System.Linq;

namespace LunaServer.Message.Reader
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
                var data = message as PlayerColorSetMsgData;
                {
                    if (data != null && data.PlayerName == client.PlayerName)
                    {
                        client.PlayerColor = data.Color;
                        MessageQueuer.RelayMessage<PlayerColorSrvMsg>(client, data);
                    }
                }
                break;
            }
        }

        private static void SendAllPlayerColors(ClientStructure client)
        {
            var sendColors = new Dictionary<string, string>();

            foreach (var otherClient in ClientRetriever.GetAuthenticatedClients().Where(c => !Equals(c, client) && c.PlayerColor != null))
                sendColors[otherClient.PlayerName] = otherClient.PlayerColor;

            var newMessageData = new PlayerColorReplyMsgData
            {
                Count = sendColors.Count,
                PlayersColors = sendColors.ToArray()
            };
            MessageQueuer.SendToClient<PlayerColorSrvMsg>(client, newMessageData);
        }
    }
}