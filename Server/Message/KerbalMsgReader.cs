using System;
using LmpCommon.Message.Data.Kerbal;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using Server.Client;
using Server.Message.Base;
using Server.System;

namespace Server.Message
{
    public class KerbalMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = message.Data as KerbalBaseMsgData;
            switch (data?.KerbalMessageType)
            {
                case KerbalMessageType.Request:
                    KerbalSystem.HandleKerbalsRequest(client);
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                case KerbalMessageType.Proto:
                    KerbalSystem.HandleKerbalProto(client, (KerbalProtoMsgData)data);
                    break;
                case KerbalMessageType.Remove:
                    KerbalSystem.HandleKerbalRemove(client, (KerbalRemoveMsgData)data);

                    break;
                default:
                    throw new NotImplementedException("Kerbal type not implemented");
            }
        }
    }
}
