using System;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.CraftLibrary;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.CraftLibrary
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

            // Write craft data
            msgData.Craft.CraftNumBytes = craft.CraftNumBytes;

            if (msgData.Craft.CraftData.Length < craft.CraftNumBytes)
                msgData.Craft.CraftData = new byte[craft.CraftNumBytes];

            Array.Copy(craft.CraftData, msgData.Craft.CraftData, craft.CraftNumBytes);

            // Write craft info data
            msgData.Craft.CraftInfoNumBytes = craft.CraftInfoNumBytes;

            if (msgData.Craft.CraftInfoData.Length < craft.CraftInfoNumBytes)
                msgData.Craft.CraftInfoData = new byte[craft.CraftInfoNumBytes];

            Array.Copy(craft.CraftInfoData, msgData.Craft.CraftInfoData, craft.CraftInfoNumBytes);

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
