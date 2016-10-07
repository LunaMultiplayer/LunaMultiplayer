using LunaCommon.Enums;

namespace LunaCommon.Message.Interface
{
    public interface IServerMessageBase : IMessageBase
    {
        ServerMessageType MessageType { get; }
    }
}