using LunaClient.Base;

namespace LunaClient.Systems.ShareTechnology
{
    public class ShareTechnologyEvents : SubSystem<ShareTechnologySystem>
    {
        public void TechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data)
        {
            if (System.IgnoreEvents || data.target != RDTech.OperationResult.Successful) return;

            LunaLog.Log($"Relaying unlocked tech: {data.host.techID}");
            System.MessageSender.SendTechnologyMessage(data.host.techID);
        }
    }
}
