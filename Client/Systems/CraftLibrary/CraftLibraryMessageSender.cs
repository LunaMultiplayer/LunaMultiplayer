using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibraryMessageSender : SubSystem<CraftLibrarySystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<CraftLibraryCliMsg>(msg)));
        }

        public void SendCraftMsg(CraftEntry craft)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryDataMsgData>();
            msgData.Craft.FolderName = craft.FolderName;
            msgData.Craft.CraftName = craft.CraftName;
            msgData.Craft.CraftType = craft.CraftType;

            msgData.Craft.NumBytes = craft.CraftNumBytes;

            if (msgData.Craft.Data.Length < craft.CraftNumBytes)
                msgData.Craft.Data = new byte[craft.CraftNumBytes];

            Array.Copy(craft.CraftData, msgData.Craft.Data, craft.CraftNumBytes);

            SendMessage(msgData);
        }

        public void SendRequestFoldersMsg()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryFoldersRequestMsgData>();
            SendMessage(msgData);
        }

        public void SendRequestCraftListMsg(string folderName)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryListRequestMsgData>();
            msgData.FolderName = folderName;

            SendMessage(msgData);
        }

        public void SendRequestCraftMsg(CraftBasicEntry craft)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryDownloadRequestMsgData>();
            msgData.CraftRequested.FolderName = craft.FolderName;
            msgData.CraftRequested.CraftName = craft.CraftName;
            msgData.CraftRequested.CraftType = craft.CraftType;

            SendMessage(msgData);
        }

        public void SendDeleteCraftMsg(CraftBasicEntry craft)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryDeleteRequestMsgData>();
            msgData.CraftToDelete.FolderName = craft.FolderName;
            msgData.CraftToDelete.CraftName = craft.CraftName;
            msgData.CraftToDelete.CraftType = craft.CraftType;

            SendMessage(msgData);
        }
    }
}
