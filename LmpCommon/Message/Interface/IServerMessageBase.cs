using LmpCommon.Enums;

namespace LmpCommon.Message.Interface
{
    public interface IServerMessageBase : IMessageBase
    {
        ServerMessageType MessageType { get; }
    }
}