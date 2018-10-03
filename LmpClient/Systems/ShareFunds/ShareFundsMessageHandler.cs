using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;

namespace LmpClient.Systems.ShareFunds
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
            System.SetFundsWithoutTriggeringEvent(funds);
            LunaLog.Log($"FundsUpdate received - funds changed to: {funds}");
        }
    }
}
