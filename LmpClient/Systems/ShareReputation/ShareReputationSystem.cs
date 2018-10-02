using LmpClient.Events;
using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;

namespace LmpClient.Systems.ShareReputation
{
    public class ShareReputationSystem : ShareProgressBaseSystem<ShareReputationSystem, ShareReputationMessageSender, ShareReputationMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareReputationSystem);

        private ShareReputationEvents ShareReputationEvents { get; } = new ShareReputationEvents();

        private float _lastReputation;

        //This queue system is not used because we use one big queue in ShareCareerSystem for this system.
        protected override bool ShareSystemReady => true;

        protected override GameMode RelevantGameModes => GameMode.Career;

        public bool Reverting { get; set; }

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnReputationChanged.Add(ShareReputationEvents.ReputationChanged);

            RevertEvent.onRevertingToLaunch.Add(ShareReputationEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Add(ShareReputationEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Add(ShareReputationEvents.LevelLoaded);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnReputationChanged.Remove(ShareReputationEvents.ReputationChanged);

            RevertEvent.onRevertingToLaunch.Remove(ShareReputationEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Remove(ShareReputationEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Remove(ShareReputationEvents.LevelLoaded);

            _lastReputation = 0;
            Reverting = false;
        }
        
        public override void SaveState()
        {
            base.SaveState();
            _lastReputation = Reputation.Instance.reputation;
        }

        public override void RestoreState()
        {
            base.RestoreState();
            Reputation.Instance.SetReputation(_lastReputation, TransactionReasons.None);
        }

        public void SetReputationWithoutTriggeringEvent(float reputation)
        {
            if (!CurrentGameModeIsRelevant) return;

            StartIgnoringEvents();
            Reputation.Instance.SetReputation(reputation, TransactionReasons.None);
            StopIgnoringEvents();
        }
    }
}
