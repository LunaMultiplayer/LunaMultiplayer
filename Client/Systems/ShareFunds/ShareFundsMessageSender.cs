using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.ShareFunds
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
