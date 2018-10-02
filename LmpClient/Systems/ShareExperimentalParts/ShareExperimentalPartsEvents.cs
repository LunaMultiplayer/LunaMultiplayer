using LmpClient.Base;

namespace LmpClient.Systems.ShareExperimentalParts
{
    public class ShareExperimentalPartsEvents : SubSystem<ShareExperimentalPartsSystem>
    {
        public void ExperimentalPartRemoved(AvailablePart part, int count)
        {
            if (System.IgnoreEvents) return;

            LunaLog.Log($"Relaying experimental part added: part: {part.name} count: {count}");
            System.MessageSender.SendExperimentalPartMessage(part.name, count);
        }

        public void ExperimentalPartAdded(AvailablePart part, int count)
        {
            if (System.IgnoreEvents) return;

            LunaLog.Log($"Relaying experimental part removed: part: {part.name} count: {count}");
            System.MessageSender.SendExperimentalPartMessage(part.name, count);
        }
    }
}
