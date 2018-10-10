using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;

namespace LmpClient.Systems.ShareUpgradeableFacilities
{
    public class ShareUpgradeableFacilitiesSystem : ShareProgressBaseSystem<ShareUpgradeableFacilitiesSystem, ShareUpgradeableFacilitiesMessageSender, ShareUpgradeableFacilitiesMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareUpgradeableFacilitiesSystem);

        private ShareUpgradeableFacilitiesEvents ShareUpgradeableFacilitiesEvents { get; } = new ShareUpgradeableFacilitiesEvents();

        //This queue system is not used because we use one big queue in ShareCareerSystem for this system.
        protected override bool ShareSystemReady => true;

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
