using System;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Log;
using LunaServer.Message.Reader.Base;
using LunaServer.Message.ReceiveHandlers;
using LunaServer.Server;
using LunaServer.System;

namespace LunaServer.Message.Reader
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

            MessageQueuer.SendToClient<HandshakeSrvMsg>(client, new HandshakeChallengeMsgData { Challenge = client.Challange });
        }
    }
}
