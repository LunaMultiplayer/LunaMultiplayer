using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.ShareReputation
{
    public class ShareReputationMessageSender : SubSystem<ShareReputationSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }


        public void SendReputationMsg(float reputation, string reason)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressReputationMsgData>();
            msgData.Reputation = reputation;
            msgData.Reason = reason;

            SendMessage(msgData);
        }
    }
}
