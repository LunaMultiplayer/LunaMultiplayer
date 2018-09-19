using LmpCommon.Message.Interface;

namespace LmpClient.Base.Interface
{
    public interface IMessageSender
    {
        void SendMessage(IMessageData msg);
    }
}