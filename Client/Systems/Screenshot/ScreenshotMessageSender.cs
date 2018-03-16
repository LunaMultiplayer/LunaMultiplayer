using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Screenshot;
using LunaCommon.Message.Interface;
using System;

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
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotUploadMsgData>();

            msgData.NumBytes = data.Length;

            if(msgData.Data.Length < msgData.NumBytes)
                msgData.Data = new byte[msgData.NumBytes];

            Array.Copy(data, msgData.Data, msgData.NumBytes);

            SendMessage(msgData);
        }
    }
}