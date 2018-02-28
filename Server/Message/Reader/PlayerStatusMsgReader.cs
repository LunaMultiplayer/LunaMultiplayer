using LunaCommon.Message.Data.PlayerStatus;
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
    public class PlayerStatusMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = message.Data as PlayerStatusBaseMsgData;
            switch (messageData?.PlayerStatusMessageType)
            {
                case PlayerStatusMessageType.Request:
                    SendOtherPlayerStatusesToNewPlayer(client);
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                case PlayerStatusMessageType.Set:
                    var data = (PlayerStatusSetMsgData)messageData;
                    if (data.PlayerStatus.PlayerName != client.PlayerName) return;

                    client.PlayerStatus.VesselText = data.PlayerStatus.VesselText;
                    client.PlayerStatus.StatusText = data.PlayerStatus.StatusText;
                    MessageQueuer.RelayMessage<PlayerStatusSrvMsg>(client, data);
                    break;
            }
        }

        private static void SendOtherPlayerStatusesToNewPlayer(ClientStructure client)
        {
            var otherClientsStatus = ClientRetriever.GetAuthenticatedClients().Where(c => !Equals(c, client)).Select(c=> new PlayerStatusInfo
            {
                PlayerName = c.PlayerName,
                StatusText = c.PlayerStatus.StatusText,
                VesselText = c.PlayerStatus.VesselText
            }).ToArray();

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<PlayerStatusReplyMsgData>();
            msgData.PlayerStatus = otherClientsStatus;
            msgData.PlayerStatusCount = msgData.PlayerStatus.Length;

            MessageQueuer.SendToClient<PlayerStatusSrvMsg>(client, msgData);
        }
    }
}