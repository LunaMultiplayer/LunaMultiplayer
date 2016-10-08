using LunaCommon.Enums;

namespace LunaCommon.Message.Interface
{
    public interface IMasterServerMessageBase : IMessageBase
    {
        MasterServerMessageType MessageType { get; }
    }
}