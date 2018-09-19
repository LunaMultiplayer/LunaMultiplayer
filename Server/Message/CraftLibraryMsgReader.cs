using System;
using LmpCommon.Message.Data.CraftLibrary;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using Server.Client;
using Server.Message.Base;
using Server.System;

namespace Server.Message
{
    public class CraftLibraryMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (CraftLibraryBaseMsgData)message.Data;

            switch (data.CraftMessageType)
            {
                case CraftMessageType.FoldersRequest:
                    CraftLibrarySystem.SendCraftFolders(client);
                    break;
                case CraftMessageType.ListRequest:
                    CraftLibrarySystem.SendCraftList(client, (CraftLibraryListRequestMsgData)data);
                    break;
                case CraftMessageType.DownloadRequest:
                    CraftLibrarySystem.SendCraft(client, (CraftLibraryDownloadRequestMsgData)data);
                    break;
                case CraftMessageType.DeleteRequest:
                    CraftLibrarySystem.DeleteCraft(client, (CraftLibraryDeleteRequestMsgData)data);
                    break;
                case CraftMessageType.CraftData:
                    CraftLibrarySystem.SaveCraft(client, (CraftLibraryDataMsgData)data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
