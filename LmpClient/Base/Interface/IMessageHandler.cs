using System.Collections.Concurrent;
using LmpCommon.Message.Interface;

namespace LmpClient.Base.Interface
{
    public interface IMessageHandler
    {
        ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; }
        void HandleMessage(IServerMessageBase msg);
    }
}