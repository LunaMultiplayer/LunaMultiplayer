using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareProgress;
using LunaClient.Systems.ShareScience;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareReputation
{
    public class ShareReputationSystem : ShareProgressBaseSystem<ShareReputationSystem, ShareReputationMessageSender, ShareReputationMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareReputationSystem);

        private ShareReputationEvents ShareReputationEvents { get; } = new ShareReputationEvents();

        private float _lastReputation;

        protected override void NetworkEventHandler(ClientState data)
        {
            if (data <= ClientState.Disconnected)
            {
                Enabled = false;
            }

            if (data == ClientState.Running && SettingsSystem.ServerSettings.ShareProgress &&
                (SettingsSystem.ServerSettings.GameMode == GameMode.Science ||
                 SettingsSystem.ServerSettings.GameMode == GameMode.Career))
            {
                Enabled = true;
            }
        }

        protected override void OnEnabled()
        {
            base.OnEnabled();
            
            _lastReputation = 0;

            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            GameEvents.OnReputationChanged.Add(ShareReputationEvents.ReputationChanged);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            GameEvents.OnReputationChanged.Remove(ShareReputationEvents.ReputationChanged);
        }

        protected override bool ActionDependencyReady()
        {
            return (Reputation.Instance != null);
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
