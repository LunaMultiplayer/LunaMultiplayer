using LmpClient.Base;

namespace LmpClient.Systems.ShareScienceSubject
{
    public class ShareScienceSubjectEvents : SubSystem<ShareScienceSubjectSystem>
    {
        public void ScienceRecieved(float dataAmount, ScienceSubject subject, ProtoVessel source, bool reverseEngineered)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendScienceSubjectMessage(subject);
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
