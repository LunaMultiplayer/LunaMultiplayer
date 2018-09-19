using LmpCommon.Enums;

namespace LmpCommon.Message.Interface
{
    public interface IMasterServerMessageBase : IMessageBase
    {
        MasterServerMessageType MessageType { get; }
    }
}