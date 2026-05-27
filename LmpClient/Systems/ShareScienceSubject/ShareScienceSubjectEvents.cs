using LmpClient.Base;

namespace LmpClient.Systems.ShareScienceSubject
{
    public class ShareScienceSubjectEvents : SubSystem<ShareScienceSubjectSystem>
    {
        public void ScienceRecieved(float dataAmount, ScienceSubject subject, ProtoVessel source, bool reverseEngineered)
        {
            if (System.IgnoreEvents) return;

            // reverseEngineered == true means the sample was physically recovered (not transmitted)
            var wasTransmitted = !reverseEngineered;
            System.MessageSender.SendScienceSubjectMessage(subject, wasTransmitted);
        }

        public void RevertingDetected()
        {
            // Send the pre-flight snapshot NOW, before scene transitions clear the science state.
            // The server applies merge logic, so other players' science is never rolled back.
            System.SendRevertSnapshot();

            System.Reverting = true;
            System.StartIgnoringEvents();
        }

        public void RevertingToEditorDetected(EditorFacility data)
        {
            System.SendRevertSnapshot();

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
