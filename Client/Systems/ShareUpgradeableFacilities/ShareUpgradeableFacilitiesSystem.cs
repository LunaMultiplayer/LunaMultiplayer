using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;
using UniLinq;
using Upgradeables;

namespace LunaClient.Systems.ShareUpgradeableFacilities
{
    public class ShareUpgradeableFacilitiesSystem : ShareProgressBaseSystem<ShareUpgradeableFacilitiesSystem, ShareUpgradeableFacilitiesMessageSender, ShareUpgradeableFacilitiesMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareUpgradeableFacilitiesSystem);

        private ShareUpgradeableFacilitiesEvents ShareUpgradeableFacilitiesEvents { get; } = new ShareUpgradeableFacilitiesEvents();

        protected override bool ShareSystemReady => UnityEngine.Object.FindObjectsOfType<UpgradeableFacility>().Any();

        protected override GameMode RelevantGameModes => GameMode.Career;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnKSCFacilityUpgrading.Add(ShareUpgradeableFacilitiesEvents.FacilityUpgraded);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnKSCFacilityUpgrading.Remove(ShareUpgradeableFacilitiesEvents.FacilityUpgraded);
        }
    }
}
