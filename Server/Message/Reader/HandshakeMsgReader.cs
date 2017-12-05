using System;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Message.Reader.Base;
using Server.Message.ReceiveHandlers;
using Server.Server;
using Server.System;

namespace Server.Message.Reader
{
    public class HandshakeMsgReader : ReaderBase
    {
        private static readonly HandshakeHandler HandshakeHandler = new HandshakeHandler();

        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as HandshakeBaseMsgData;
            switch (message?.HandshakeMessageType)
            {
                case HandshakeMessageType.Request:
                    SetAndSendHandshakeChallangeMessage(client);
                    break;
                case HandshakeMessageType.Response:
                    var data = (HandshakeResponseMsgData)message;
                    try
                    {
                        HandshakeHandler.HandleHandshakeResponse(client, data);
                    }
                    catch (Exception e)
                    {
                        LunaLog.Debug($"Error in HANDSHAKE_REQUEST from {data.PlayerName}: {e}");
                        HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.MalformedHandshake, "Malformed handshake");
                    }
                    break;
                default:
                    throw new NotImplementedException("Warp Type not implemented");
            }
        }

        private static void SetAndSendHandshakeChallangeMessage(ClientStructure client)
        {
            client.Challange = new byte[1024];
            new Random().NextBytes(client.Challange);

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<HandshakeChallengeMsgData>();
            msgData.Challenge = client.Challange;

            MessageQueuer.SendToClient<HandshakeSrvMsg>(client, msgData);
        }
    }
}
