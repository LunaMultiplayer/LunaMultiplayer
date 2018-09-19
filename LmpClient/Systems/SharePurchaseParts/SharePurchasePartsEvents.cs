using LmpClient.Base;

namespace LmpClient.Systems.SharePurchaseParts
{
    public class SharePurchasePartsEvents : SubSystem<SharePurchasePartsSystem>
    {
        public void PartPurchased(AvailablePart part)
        {
            if (System.IgnoreEvents) return;

            var techState = ResearchAndDevelopment.Instance.GetTechState(part.TechRequired);
            if (techState != null)
            {
                LunaLog.Log($"Relaying part purchased on tech: {techState.techID}; part: {part.name}");
                System.MessageSender.SendPartPurchasedMessage(techState.techID, part.name);
            }
        }
    }
}
