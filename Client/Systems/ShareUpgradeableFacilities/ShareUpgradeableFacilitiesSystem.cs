using LunaClient.Systems.SettingsSys;
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

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnEnabled();
            GameEvents.OnKSCFacilityUpgrading.Add(ShareUpgradeableFacilitiesEvents.FacilityUpgraded);
        }

        protected override void OnDisabled()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnDisabled();
            GameEvents.OnKSCFacilityUpgrading.Remove(ShareUpgradeableFacilitiesEvents.FacilityUpgraded);
        }
    }
}
