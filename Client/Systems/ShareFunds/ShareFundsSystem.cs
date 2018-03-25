using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareFunds
{
    public class ShareFundsSystem : ShareProgressBaseSystem<ShareFundsSystem, ShareFundsMessageSender, ShareFundsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareFundsSystem);

        private ShareFundsEvents ShareFundsEvents { get; } = new ShareFundsEvents();

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

        protected override bool ActionDependencyReady()
        {
            return (Funding.Instance != null);
        }

        public override void SaveState()
        {
            _lastFunds = Funding.Instance.Funds;
        }

        public override void RestoreState()
        {
            Funding.Instance.SetFunds(_lastFunds, TransactionReasons.None);
        }
    }
}
