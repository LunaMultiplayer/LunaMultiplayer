using System;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;

namespace Server.Message.Reader
{
    public class WarpControlMsgReader : ReaderBase
    {
        private static readonly WarpSystemReceiver WarpReceiver = new WarpSystemReceiver();

        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as WarpBaseMsgData;
            switch (message?.WarpMessageType)
            {
                case WarpMessageType.NewSubspace:
                    WarpReceiver.HandleNewSubspace(client, (WarpNewSubspaceMsgData)messageData);
                    break;
                case WarpMessageType.ChangeSubspace:
                    WarpReceiver.HandleChangeSubspace(client, (WarpChangeSubspaceMsgData)messageData);
                    break;
                case WarpMessageType.SubspacesRequest:
                    WarpSystemSender.SendAllSubspaces(client);
                    break;
                default:
                    throw new NotImplementedException("Warp Type not implemented");
            }
        }
    }
}