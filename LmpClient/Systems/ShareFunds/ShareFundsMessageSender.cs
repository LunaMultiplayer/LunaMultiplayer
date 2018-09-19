using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.ShareFunds
{
    public class ShareFundsMessageSender : SubSystem<ShareFundsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendFundsMessage(double funds, string reason)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressFundsMsgData>();
            msgData.Funds = funds;
            msgData.Reason = reason;
            SendMessage(msgData);
        }
    }
}
