using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;
using System;

namespace Server.Message.Reader
{
    public class KerbalMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = message.Data as KerbalBaseMsgData;
            switch (messageData?.KerbalMessageType)
            {
                case KerbalMessageType.Request:
                    KerbalSystem.HandleKerbalsRequest(client);
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                case KerbalMessageType.Proto:
                    KerbalSystem.HandleKerbalProto(client, (KerbalProtoMsgData)messageData);
                    break;
                case KerbalMessageType.Remove:
                    KerbalSystem.HandleKerbalRemove(client, (KerbalRemoveMsgData)messageData);

                    break;
                default:
                    throw new NotImplementedException("Kerbal type not implemented");
            }
        }
    }
}