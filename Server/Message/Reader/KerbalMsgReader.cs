using System;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;

namespace Server.Message.Reader
{
    public class KerbalMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as KerbalBaseMsgData;
            switch (message?.KerbalMessageType)
            {
                case KerbalMessageType.Request:
                    KerbalSystem.HandleKerbalsRequest(client);
                    break;
                case KerbalMessageType.Proto:
                    KerbalSystem.HandleKerbalProto(client, (KerbalProtoMsgData)message);
                    break;
                case KerbalMessageType.Remove:
                    KerbalSystem.HandleKerbalRemove(client, (KerbalRemoveMsgData)message);

                    break;
                default:
                    throw new NotImplementedException("Kerbal type not implemented");
            }
        }
    }
}