using LunaClient.Base;

namespace LunaClient.Systems.ShareScienceSubject
{
    public class ShareScienceSubjectEvents : SubSystem<ShareScienceSubjectSystem>
    {
        public void ScienceRecieved(float dataAmount, ScienceSubject subject, ProtoVessel source, bool reverseEngineered)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendScienceSubjectMessage(subject);
        }
    }
}
