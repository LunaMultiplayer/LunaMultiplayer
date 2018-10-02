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

        public void RevertingDetected()
        {
            System.Reverting = true;
            System.StartIgnoringEvents();
        }

        public void RevertingToEditorDetected(EditorFacility data)
        {
            System.Reverting = true;
            System.StartIgnoringEvents();
        }

        public void LevelLoaded(GameScenes data)
        {
            if (System.Reverting)
            {
                System.Reverting = false;
                System.StopIgnoringEvents(true);
            }
        }
    }
}
