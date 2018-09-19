using LmpCommon.Enums;

namespace LmpCommon.Message.Interface
{
    public interface IClientMessageBase : IMessageBase
    {
        ClientMessageType MessageType { get; }
        bool Handled { get; set; }
    }
}