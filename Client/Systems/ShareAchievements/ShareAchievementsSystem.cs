using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareAchievements
{
    public class ShareAchievementsSystem : ShareProgressBaseSystem<ShareAchievementsSystem, ShareAchievementsMessageSender, ShareAchievementsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareAchievementsSystem);

        private ShareAchievementsEvents ShareAchievementsEvents { get; } = new ShareAchievementsEvents();

        //This queue system is not used because we use one big queue in ShareCareerSystem for this system.
        protected override bool ShareSystemReady => true;

        //We don't need to synchronize achievements in science mode because they have no effect and are not shown to the user.
        //They will only appear in the debug console.
        protected override GameMode RelevantGameModes => GameMode.Career;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnProgressReached.Add(ShareAchievementsEvents.AchievementReached);
            GameEvents.OnProgressComplete.Add(ShareAchievementsEvents.AchievementCompleted);
            GameEvents.OnProgressAchieved.Add(ShareAchievementsEvents.AchievementAchieved);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnProgressReached.Remove(ShareAchievementsEvents.AchievementReached);
            GameEvents.OnProgressComplete.Remove(ShareAchievementsEvents.AchievementCompleted);
            GameEvents.OnProgressAchieved.Remove(ShareAchievementsEvents.AchievementAchieved);
        }
    }
}
