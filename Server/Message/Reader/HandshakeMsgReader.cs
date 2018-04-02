using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;
using System;

namespace Server.Message.Reader
{
    public class HandshakeMsgReader : ReaderBase
    {
        private static readonly HandshakeSystem HandshakeHandler = new HandshakeSystem();

        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = message.Data as HandshakeBaseMsgData;
            switch (data?.HandshakeMessageType)
            {
                case HandshakeMessageType.Request:
                    HandshakeHandler.HandleHandshakeRequest(client, (HandshakeRequestMsgData)data);
                    break;
                default:
                    throw new NotImplementedException("Handshake type not implemented");
            }
            
            message.Recycle();
        }
    }
}
