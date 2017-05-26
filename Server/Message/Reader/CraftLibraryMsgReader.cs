using System;
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
                case CraftMessageType.UploadFile:
                    CraftLibraryHandler.HandleUploadFileMessage(client, (CraftLibraryUploadMsgData)message);
                    break;
                case CraftMessageType.RequestFile:
                    CraftLibraryHandler.HandleRequestFileMessage(client, (CraftLibraryRequestMsgData)message);
                    break;
                case CraftMessageType.DeleteFile:
                    CraftLibraryHandler.HandleDeleteFileMessage(client, (CraftLibraryDeleteMsgData)message);
                    break;
                case CraftMessageType.ListRequest:
                    CraftLibraryHandler.SendCraftList(client);
                    break;
                case CraftMessageType.ListReply:
                    //Do not handle this
                    break;
                case CraftMessageType.RespondFile:
                    //Do not handle this
                    break;
                case CraftMessageType.AddFile:
                    //Do not handle this
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}