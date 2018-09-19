using LmpClient.Base;

namespace LmpClient.Systems.ShareTechnology
{
    public class ShareTechnologyEvents : SubSystem<ShareTechnologySystem>
    {
        public void TechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data)
        {
            if (System.IgnoreEvents || data.target != RDTech.OperationResult.Successful) return;

            LunaLog.Log($"Relaying researched technology: {data.host.techID}");
            System.MessageSender.SendTechnologyMessage(data.host);
        }
    }
}
