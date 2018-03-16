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
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ScreenshotDataMsgData>();

            msgData.Screenshot.NumBytes = data.Length;

            if(msgData.Screenshot.Data.Length < msgData.Screenshot.NumBytes)
                msgData.Screenshot.Data = new byte[msgData.Screenshot.NumBytes];

            Array.Copy(data, msgData.Screenshot.Data, msgData.Screenshot.NumBytes);

            SendMessage(msgData);
        }
    }
}