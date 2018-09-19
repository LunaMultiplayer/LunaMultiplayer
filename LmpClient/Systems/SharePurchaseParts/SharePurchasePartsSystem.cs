using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;

namespace LmpClient.Systems.SharePurchaseParts
{
    public class SharePurchasePartsSystem : ShareProgressBaseSystem<SharePurchasePartsSystem, SharePurchasePartsMessageSender, SharePurchasePartsMessageHandler>
    {
        public override string SystemName { get; } = nameof(SharePurchasePartsSystem);

        private SharePurchasePartsEvents SharePurchasePartsEvents { get; } = new SharePurchasePartsEvents();

        protected override bool ShareSystemReady => ResearchAndDevelopment.Instance != null && Funding.Instance != null;

        protected override GameMode RelevantGameModes => GameMode.Career;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            if (HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch) return;

            GameEvents.OnPartPurchased.Add(SharePurchasePartsEvents.PartPurchased);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnPartPurchased.Remove(SharePurchasePartsEvents.PartPurchased);
        }
    }
}
