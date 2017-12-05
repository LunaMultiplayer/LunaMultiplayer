using System.Linq;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Context;
using Server.Message.Reader.Base;
using Server.Server;

namespace Server.Message.Reader
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
                    if (data.PlayerName != client.PlayerName) return;

                    client.PlayerStatus.VesselText = data.VesselText;
                    client.PlayerStatus.StatusText = data.StatusText;
                    MessageQueuer.RelayMessage<PlayerStatusSrvMsg>(client, data);
                    break;
            }
        }

        private static void SendOtherPlayerStatusToNewPlayer(ClientStructure client)
        {
            var otherClients = ClientRetriever.GetAuthenticatedClients().Where(c => !Equals(c, client)).ToArray();

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<PlayerStatusReplyMsgData>();
            msgData.PlayerName = otherClients.Select(c => c.PlayerName).ToArray();
            msgData.StatusText = otherClients.Select(c => c.PlayerStatus.StatusText).ToArray();
            msgData.VesselText = otherClients.Select(c => c.PlayerStatus.VesselText).ToArray();
            
            MessageQueuer.SendToClient<PlayerStatusSrvMsg>(client, msgData);
        }
    }
}