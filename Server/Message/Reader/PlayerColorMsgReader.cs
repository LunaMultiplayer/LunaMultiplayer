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
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = message.Data as PlayerColorBaseMsgData;
            switch (messageData?.PlayerColorMessageType)
            {
                case PlayerColorMessageType.Request:
                    SendAllPlayerColors(client);

                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                case PlayerColorMessageType.Set:
                    var data = (PlayerColorSetMsgData)messageData;
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