using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareAchievements
{
    public class ShareAchievementsSystem : ShareProgressBaseSystem<ShareAchievementsSystem, ShareAchievementsMessageSender, ShareAchievementsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareAchievementsSystem);

        private ShareAchievementsEvents ShareAchievementsEvents { get; } = new ShareAchievementsEvents();

        protected override bool ShareSystemReady => ProgressTracking.Instance != null && Funding.Instance != null && ResearchAndDevelopment.Instance != null &&
                                                    Reputation.Instance != null;

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            base.OnEnabled();

            GameEvents.OnProgressReached.Add(ShareAchievementsEvents.AchievementReached);
            GameEvents.OnProgressComplete.Add(ShareAchievementsEvents.AchievementCompleted);
            GameEvents.OnProgressAchieved.Add(ShareAchievementsEvents.AchievementAchieved);
        }

        protected override void OnDisabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            base.OnDisabled();

            GameEvents.OnProgressReached.Remove(ShareAchievementsEvents.AchievementReached);
            GameEvents.OnProgressComplete.Remove(ShareAchievementsEvents.AchievementCompleted);
            GameEvents.OnProgressAchieved.Remove(ShareAchievementsEvents.AchievementAchieved);
        }

        public override void SaveState()
        {
            //We don't need this.
        }

        public override void RestoreState()
        {
            //We don't need this.
        }
    }
}
