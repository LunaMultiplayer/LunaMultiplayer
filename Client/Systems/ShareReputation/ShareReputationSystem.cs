using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareReputation
{
    public class ShareReputationSystem : ShareProgressBaseSystem<ShareReputationSystem, ShareReputationMessageSender, ShareReputationMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareReputationSystem);

        private ShareReputationEvents ShareReputationEvents { get; } = new ShareReputationEvents();

        private float _lastReputation;

        //This queue system is not used because we use one big queue in ShareCareerSystem for this system.
        protected override bool ShareSystemReady => true;

        protected override GameMode RelevantGameModes => GameMode.Career;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnReputationChanged.Add(ShareReputationEvents.ReputationChanged);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnReputationChanged.Remove(ShareReputationEvents.ReputationChanged);
            _lastReputation = 0;
        }
        
        public override void SaveState()
        {
            _lastReputation = Reputation.Instance.reputation;
        }

        public override void RestoreState()
        {
            Reputation.Instance.SetReputation(_lastReputation, TransactionReasons.None);
        }
    }
}
