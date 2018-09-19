using LmpClient.Base;

namespace LmpClient.Systems.ShareReputation
{
    public class ShareReputationEvents : SubSystem<ShareReputationSystem>
    {
        public void ReputationChanged(float reputation, TransactionReasons reason)
        {
            if (System.IgnoreEvents) return;

            LunaLog.Log($"Reputation changed to: {reputation} reason: {reason}");
            System.MessageSender.SendReputationMsg(reputation, reason.ToString());
        }
    }
}
