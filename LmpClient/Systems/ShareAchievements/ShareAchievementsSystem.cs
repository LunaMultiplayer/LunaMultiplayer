using Harmony;
using LmpClient.Events;
using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;
using System.Linq;

namespace LmpClient.Systems.ShareAchievements
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

        public ConfigNode _lastAchievements;

        public bool Reverting { get; set; }

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnProgressReached.Add(ShareAchievementsEvents.AchievementReached);
            GameEvents.OnProgressComplete.Add(ShareAchievementsEvents.AchievementCompleted);
            GameEvents.OnProgressAchieved.Add(ShareAchievementsEvents.AchievementAchieved);

            RevertEvent.onRevertingToLaunch.Add(ShareAchievementsEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Add(ShareAchievementsEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Add(ShareAchievementsEvents.LevelLoaded);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnProgressReached.Remove(ShareAchievementsEvents.AchievementReached);
            GameEvents.OnProgressComplete.Remove(ShareAchievementsEvents.AchievementCompleted);
            GameEvents.OnProgressAchieved.Remove(ShareAchievementsEvents.AchievementAchieved);

            RevertEvent.onRevertingToLaunch.Remove(ShareAchievementsEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Remove(ShareAchievementsEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Remove(ShareAchievementsEvents.LevelLoaded);

            _lastAchievements = null;
            Reverting = false;
        }

        public override void SaveState()
        {
            base.SaveState();
            _lastAchievements = new ConfigNode();
            ProgressTracking.Instance?.achievementTree.Save(_lastAchievements);
        }

        public override void RestoreState()
        {
            base.RestoreState();
            if (ProgressTracking.Instance == null)
            {
                //The instance will be null when reverting to editor so we must restore the old data into the proto
                //This happens because in ProgressTracking class, the KSPScenario attribute doesn't include 
                //GameScenes.Editor into it's values :(
                var achievementsScn = HighLogic.CurrentGame.scenarios.FirstOrDefault(s => s.moduleName == "ProgressTracking");
                var moduleValues = Traverse.Create(achievementsScn).Field<ConfigNode>("moduleValues").Value;
                
                var progressNode = moduleValues.GetNode("Progress");
                progressNode.ClearNodes();

                foreach (var node in _lastAchievements.GetNodes())
                {
                    progressNode.AddNode(node);
                }
            }
            else
            {
                ProgressTracking.Instance.achievementTree.Load(_lastAchievements);
            }
        }
    }
}
