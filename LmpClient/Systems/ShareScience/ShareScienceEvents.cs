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
