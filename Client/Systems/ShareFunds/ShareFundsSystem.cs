using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareFunds
{
    public class ShareFundsSystem : MessageSystem<ShareFundsSystem, ShareFundsMessageSender, ShareFundsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareFundsSystem);

        private ShareFundsEvents ShareFundsEvents { get; } = new ShareFundsEvents();
        public bool IgnoreEvents { get; set; }
        private double _lastFunds;

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
            _lastFunds = 0;

            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            GameEvents.OnFundsChanged.Add(ShareFundsEvents.FundsChanged);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            GameEvents.OnFundsChanged.Remove(ShareFundsEvents.FundsChanged);
        }

        public void StartIgnoringEvents()
        {
            if (Funding.Instance != null)
                _lastFunds = Funding.Instance.Funds;

            IgnoreEvents = true;
        }

        public void StopIgnoringEvents(bool restoreOldValue = false)
        {
            if (restoreOldValue && Reputation.Instance != null)
                Funding.Instance.SetFunds(_lastFunds, TransactionReasons.None);

            IgnoreEvents = false;
        }
    }
}
