using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using System.Linq;

namespace LunaServer.Message.Reader
{
    public class PlayerStatusMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as PlayerStatusBaseMsgData;
            switch (message?.PlayerStatusMessageType)
            {
                case PlayerStatusMessageType.Request:
                    SendOtherPlayerStatusToNewPlayer(client);
                    break;
                case PlayerStatusMessageType.Set:
                    var data = (PlayerStatusSetMsgData)message;
                    if (data.PlayerName == client.PlayerName)
                    {
                        client.PlayerStatus.VesselText = data.VesselText;
                        client.PlayerStatus.StatusText = data.StatusText;
                    }
                    MessageQueuer.RelayMessage<PlayerStatusSrvMsg>(client, data);
                    break;
            }
        }

        private static void SendOtherPlayerStatusToNewPlayer(ClientStructure client)
        {
            var otherClients = ClientRetriever.GetAuthenticatedClients().Where(c => !Equals(c, client)).ToArray();

            var otherPlayerStatusMsgData = new PlayerStatusReplyMsgData
            {
                PlayerName = otherClients.Select(c => c.PlayerName).ToArray(),
                StatusText = otherClients.Select(c => c.PlayerStatus.StatusText).ToArray(),
                VesselText = otherClients.Select(c => c.PlayerStatus.VesselText).ToArray()
            };

            MessageQueuer.SendToClient<PlayerStatusSrvMsg>(client, otherPlayerStatusMsgData);
        }
    }
}