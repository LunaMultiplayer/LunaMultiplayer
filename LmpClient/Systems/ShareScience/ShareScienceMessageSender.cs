using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.ShareScience
{
    public class ShareScienceMessageSender : SubSystem<ShareScienceSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendScienceMessage(float science, string reason)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressScienceMsgData>();
            msgData.Science = science;
            msgData.Reason = reason;
            System.MessageSender.SendMessage(msgData);
            LunaLog.Log($"Science changed to: {science} with reason: {reason}");
        }
    }
}
