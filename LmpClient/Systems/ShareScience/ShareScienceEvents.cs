using LmpClient.Base;

namespace LmpClient.Systems.ShareScience
{
    public class ShareScienceEvents : SubSystem<ShareScienceSystem>
    {
        public void ScienceChanged(float science, TransactionReasons reason)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendScienceMessage(science, reason.ToString());
        }
    }
}
