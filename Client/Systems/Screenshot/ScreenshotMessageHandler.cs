using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Screenshot;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Screenshot
{
    public class ScreenshotMessageHandler : SubSystem<ScreenshotSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ScreenshotBaseMsgData msgData)) return;

            switch (msgData.ScreenshotMessageType)
            {
                case ScreenshotMessageType.ScreenshotData:
                    break;
            }
        }
    }
}