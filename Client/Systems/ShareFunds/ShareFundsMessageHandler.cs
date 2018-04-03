using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.ShareFunds
{
    public class ShareFundsMessageHandler : SubSystem<ShareFundsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.FundsUpdate) return;
            
            if (msgData is ShareProgressFundsMsgData data)
            {
                var funds = data.Funds; //create a copy of the funds value so it will not change in the future.
                LunaLog.Log($"Queue FundsUpdate with: {funds}");
                System.QueueAction(() =>
                {
                    FundsUpdate(funds);
                });
            }
        }

        private static void FundsUpdate(double funds)
        {
            System.StartIgnoringEvents();
            Funding.Instance.SetFunds(funds, TransactionReasons.None);
            System.StopIgnoringEvents();
            LunaLog.Log($"FundsUpdate received - funds changed to: {funds}");
        }
    }
}
