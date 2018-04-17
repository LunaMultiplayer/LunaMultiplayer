using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.SharePurchaseParts
{
    public class SharePurchasePartsSystem : ShareProgressBaseSystem<SharePurchasePartsSystem, SharePurchasePartsMessageSender, SharePurchasePartsMessageHandler>
    {
        public override string SystemName { get; } = nameof(SharePurchasePartsSystem);

        private SharePurchasePartsEvents SharePurchasePartsEvents { get; } = new SharePurchasePartsEvents();

        protected override bool ShareSystemReady => ResearchAndDevelopment.Instance != null && Funding.Instance != null;

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox || HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch) return;

            base.OnEnabled();
            GameEvents.OnPartPurchased.Add(SharePurchasePartsEvents.PartPurchased);
        }

        protected override void OnDisabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox || HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch) return;

            base.OnDisabled();
            GameEvents.OnPartPurchased.Remove(SharePurchasePartsEvents.PartPurchased);
        }
    }
}
