using LunaCommon.Message.Data.Screenshot;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;
using System;

namespace Server.Message.Reader
{
    public class ScreenshotMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (ScreenshotBaseMsgData)message.Data;
            switch (data.ScreenshotMessageType)
            {
                case ScreenshotMessageType.ListRequest:
                    break;
                case ScreenshotMessageType.ScreenshotData:
                    ScreenshotSystem.SaveScreenshot(client, (ScreenshotDataMsgData)data);
                    break;
                case ScreenshotMessageType.FoldersRequest:
                    break;
                case ScreenshotMessageType.DownloadRequest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}