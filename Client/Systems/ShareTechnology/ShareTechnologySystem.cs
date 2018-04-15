using KSP.UI.Screens;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareTechnology
{
    public class ShareTechnologySystem : ShareProgressBaseSystem<ShareTechnologySystem, ShareTechnologyMessageSender, ShareTechnologyMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareTechnologySystem);

        private ShareTechnologyEvents ShareTechnologyEvents { get; } = new ShareTechnologyEvents();

        protected override bool ShareSystemReady => ResearchAndDevelopment.Instance != null && /*RDController.Instance != null &&*/ Funding.Instance != null;

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            base.OnEnabled();
            GameEvents.OnTechnologyResearched.Add(ShareTechnologyEvents.TechnologyResearched);
            GameEvents.OnPartPurchased.Add(ShareTechnologyEvents.PartPurchased);
        }

        protected override void OnDisabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            base.OnDisabled();
            GameEvents.OnTechnologyResearched.Remove(ShareTechnologyEvents.TechnologyResearched);
            GameEvents.OnPartPurchased.Remove(ShareTechnologyEvents.PartPurchased);
        }
    }
}
