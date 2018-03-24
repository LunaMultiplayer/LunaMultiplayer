using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareScience;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareReputation
{
    public class ShareReputationSystem : MessageSystem<ShareReputationSystem, ShareReputationMessageSender, ShareReputationMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareReputationSystem);

        private ShareReputationEvents ShareReputationEvents { get; } = new ShareReputationEvents();
        public bool IgnoreEvents { get; set; }
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

            IgnoreEvents = false;
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

        public void StartIgnoringEvents()
        {
            if (Reputation.Instance != null)
                _lastReputation = Reputation.Instance.reputation;

            IgnoreEvents = true;
        }

        public void StopIgnoringEvents(bool restoreOldValue = false)
        {
            if (restoreOldValue && Reputation.Instance != null)
                Reputation.Instance.SetReputation(_lastReputation, TransactionReasons.None);

            IgnoreEvents = false;
        }
    }
}
