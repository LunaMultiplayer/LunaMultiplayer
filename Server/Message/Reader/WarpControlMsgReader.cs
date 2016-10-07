using System;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.System;

namespace LunaServer.Message.Reader
{
    public class WarpControlMsgReader : ReaderBase
    {
        private static readonly WarpSystemReceiver WarpReceiver = new WarpSystemReceiver();

        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as WarpBaseMsgData;
            switch (message?.WarpMessageType)
            {
                case WarpMessageType.NEW_SUBSPACE:
                    WarpReceiver.HandleNewSubspace(client, (WarpNewSubspaceMsgData)messageData);
                    break;
                case WarpMessageType.CHANGE_SUBSPACE:
                    WarpReceiver.HandleChangeSubspace(client, (WarpChangeSubspaceMsgData)messageData);
                    break;
                case WarpMessageType.SUBSPACES_REQUEST:
                    WarpSystemSender.SendAllSubspaces(client);
                    break;
                default:
                    throw new NotImplementedException("Warp Type not implemented");
            }
        }
    }
}