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

        protected override bool ShareSystemReady => Funding.Instance != null;

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnEnabled();
            GameEvents.OnFundsChanged.Add(ShareFundsEvents.FundsChanged);
        }

        protected override void OnDisabled()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnDisabled();
            GameEvents.OnFundsChanged.Remove(ShareFundsEvents.FundsChanged);
            _lastFunds = 0;
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
