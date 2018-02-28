using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Message.Reader.Base;
using Server.Server;
using Server.System;
using System;

namespace Server.Message.Reader
{
    public class HandshakeMsgReader : ReaderBase
    {
        private static readonly HandshakeSystem HandshakeHandler = new HandshakeSystem();

        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = message.Data as HandshakeBaseMsgData;
            switch (messageData?.HandshakeMessageType)
            {
                case HandshakeMessageType.Request:
                    SetAndSendHandshakeChallangeMessage(client);
                    break;
                case HandshakeMessageType.Response:
                    var data = (HandshakeResponseMsgData)messageData;
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
                    throw new NotImplementedException("Handshake type not implemented");
            }

            //We don't use this message so we can recycle it
            message.Recycle();
        }

        private static void SetAndSendHandshakeChallangeMessage(ClientStructure client)
        {
            new Random().NextBytes(client.Challenge);

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<HandshakeChallengeMsgData>();
            msgData.Challenge = client.Challenge;

            MessageQueuer.SendToClient<HandshakeSrvMsg>(client, msgData);
        }
    }
}
