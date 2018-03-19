using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibraryMessageSender : SubSystem<CraftLibrarySystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<CraftLibraryCliMsg>(msg)));
        }

        public void SendCraft(byte[] data)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryDataMsgData>();

            SendMessage(msgData);
        }

        public void RequestFolders()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryFoldersRequestMsgData>();
            SendMessage(msgData);
        }

        public void RequestCraftList(string folderName)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryListRequestMsgData>();
            msgData.FolderName = folderName;

            SendMessage(msgData);
        }

        public void RequestCraft(string folderName, string craftName, CraftType craftType)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryDownloadRequestMsgData>();
            msgData.CraftRequested.FolderName = folderName;
            msgData.CraftRequested.CraftName = craftName;
            msgData.CraftRequested.CraftType = craftType;

            SendMessage(msgData);
        }
    }
}
