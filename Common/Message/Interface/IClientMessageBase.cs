using LunaCommon.Enums;

namespace LunaCommon.Message.Interface
{
    public interface IClientMessageBase : IMessageBase
    {
        ClientMessageType MessageType { get; }
        bool Handled { get; set; }
    }
}