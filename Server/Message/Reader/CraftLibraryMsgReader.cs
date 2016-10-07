using System;
using LunaCommon.Enums;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.Message.ReceiveHandlers;

namespace LunaServer.Message.Reader
{
    public class CraftLibraryMsgReader : ReaderBase
    {
        private static readonly CraftLibraryHandler CraftLibraryHandler = new CraftLibraryHandler();

        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var data = (CraftLibraryBaseMsgData)message;
            if (data.PlayerName != client.PlayerName) return;

            switch (data.CraftMessageType)
            {
                case CraftMessageType.UPLOAD_FILE:
                    CraftLibraryHandler.HandleUploadFileMessage(client, (CraftLibraryUploadMsgData)message);
                    break;
                case CraftMessageType.REQUEST_FILE:
                    CraftLibraryHandler.HandleRequestFileMessage(client, (CraftLibraryRequestMsgData)message);
                    break;
                case CraftMessageType.DELETE_FILE:
                    CraftLibraryHandler.HandleDeleteFileMessage(client, (CraftLibraryDeleteMsgData)message);
                    break;
                case CraftMessageType.LIST_REQUEST:
                    CraftLibraryHandler.SendCraftList(client);
                    break;
                case CraftMessageType.LIST_REPLY:
                    //Do not handle this
                    break;
                case CraftMessageType.RESPOND_FILE:
                    //Do not handle this
                    break;
                case CraftMessageType.ADD_FILE:
                    //Do not handle this
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}