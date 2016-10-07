using LunaCommon.Message.Interface;

namespace LunaClient.Base.Interface
{
    public interface IMessageSender
    {
        void SendMessage(IMessageData msg);
    }
}