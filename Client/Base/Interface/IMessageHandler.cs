using System.Collections.Concurrent;
using LunaCommon.Message.Interface;

namespace LunaClient.Base.Interface
{
    public interface IMessageHandler
    {
        ConcurrentQueue<IMessageData> IncomingMessages { get; set; }
        void HandleMessage(IMessageData messageData);
    }
}