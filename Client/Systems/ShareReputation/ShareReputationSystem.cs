using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareReputation
{
    public class ShareReputationSystem : ShareProgressBaseSystem<ShareReputationSystem, ShareReputationMessageSender, ShareReputationMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareReputationSystem);

        private ShareReputationEvents ShareReputationEvents { get; } = new ShareReputationEvents();

        private float _lastReputation;

        protected override bool ShareSystemReady => Reputation.Instance != null;

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnEnabled();
            GameEvents.OnReputationChanged.Add(ShareReputationEvents.ReputationChanged);
        }

        protected override void OnDisabled()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnDisabled();
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
