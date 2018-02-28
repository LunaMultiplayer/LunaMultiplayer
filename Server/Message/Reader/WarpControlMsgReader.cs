using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;
using System;

namespace Server.Message.Reader
{
    public class WarpControlMsgReader : ReaderBase
    {
        private static readonly WarpSystemReceiver WarpReceiver = new WarpSystemReceiver();

        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = message.Data as WarpBaseMsgData;
            switch (messageData?.WarpMessageType)
            {
                case WarpMessageType.NewSubspace:
                    WarpReceiver.HandleNewSubspace(client, (WarpNewSubspaceMsgData)messageData);
                    break;
                case WarpMessageType.ChangeSubspace:
                    WarpReceiver.HandleChangeSubspace(client, (WarpChangeSubspaceMsgData)messageData);
                    break;
                case WarpMessageType.SubspacesRequest:
                    WarpSystemSender.SendAllSubspaces(client);
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                default:
                    throw new NotImplementedException("Warp Type not implemented");
            }
        }
    }
}