using LmpClient.Base;

namespace LmpClient.Systems.ShareFunds
{
    public class ShareFundsEvents : SubSystem<ShareFundsSystem>
    {
        public void FundsChanged(double funds, TransactionReasons reason)
        {
            if (System.IgnoreEvents) return;

            LunaLog.Log($"Funds changed to: {funds} reason: {reason}");
            System.MessageSender.SendFundsMessage(funds, reason.ToString());
        }
    }
}
