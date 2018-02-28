using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;
using System;

namespace Server.Message.Reader
{
    public class CraftLibraryMsgReader : ReaderBase
    {
        private static readonly CraftLibrarySystem CraftLibraryHandler = new CraftLibrarySystem();

        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (CraftLibraryBaseMsgData)message.Data;
            if (data.PlayerName != client.PlayerName) return;

            switch (data.CraftMessageType)
            {
                case CraftMessageType.UploadFile:
                    CraftLibraryHandler.HandleUploadFileMessage(client, (CraftLibraryUploadMsgData)data);
                    break;
                case CraftMessageType.RequestFile:
                    CraftLibraryHandler.HandleRequestFileMessage(client, (CraftLibraryRequestMsgData)data);
                    break;
                case CraftMessageType.DeleteFile:
                    CraftLibraryHandler.HandleDeleteFileMessage(client, (CraftLibraryDeleteMsgData)data);
                    break;
                case CraftMessageType.ListRequest:
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    CraftLibrarySystem.SendCraftList(client);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}