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
                case ScreenshotMessageType.FoldersRequest:
                    ScreenshotSystem.SendScreenshotFolders(client);
                    break;
                case ScreenshotMessageType.ListRequest:
                    ScreenshotSystem.SendScreenshotList(client, (ScreenshotListRequestMsgData)data);
                    break;
                case ScreenshotMessageType.ScreenshotData:
                    ScreenshotSystem.SaveScreenshot(client, (ScreenshotDataMsgData)data);
                    break;
                case ScreenshotMessageType.DownloadRequest:
                    ScreenshotSystem.SendScreenshot(client, (ScreenshotDownloadRequestMsgData)data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}