using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Screenshot;
using LunaCommon.Message.Interface;
using LunaCommon.Time;
using System;
using UniLinq;

namespace LunaClient.Systems.Screenshot
{
    public class ScreenshotMessageSender : SubSystem<ScreenshotSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ScreenshotCliMsg>(msg)));
        }

        public void SendScreenshot(byte[] data)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotDataMsgData>();

            msgData.Screenshot.DateTaken = LunaTime.UtcNow.ToBinary();
            msgData.Screenshot.NumBytes = data.Length;

            if(msgData.Screenshot.Data.Length < msgData.Screenshot.NumBytes)
                msgData.Screenshot.Data = new byte[msgData.Screenshot.NumBytes];

            Array.Copy(data, msgData.Screenshot.Data, msgData.Screenshot.NumBytes);

            SendMessage(msgData);
        }

        public void RequestFolders()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotFoldersRequestMsgData>();
            SendMessage(msgData);
        }

        public void RequestMiniatures(string folderName)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotListRequestMsgData>();
            msgData.FolderName = folderName;

            if (System.MiniatureImages.TryGetValue(folderName, out var miniatureDictionary))
            {
                msgData.AlreadyOwnedPhotoIds = miniatureDictionary.Keys.ToArray();
                msgData.NumAlreadyOwnedPhotoIds = miniatureDictionary.Count;
            }
            else
            {
                msgData.NumAlreadyOwnedPhotoIds = 0;
            }

            SendMessage(msgData);
        }

        public void RequestImage(string folderName, long photoId)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotDownloadRequestMsgData>();
            msgData.FolderName = folderName;
            msgData.PhotoId = photoId;

            SendMessage(msgData);
        }
    }
}