using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;

namespace LmpClient.Systems.ShareFunds
{
    public class ShareFundsSystem : ShareProgressBaseSystem<ShareFundsSystem, ShareFundsMessageSender, ShareFundsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareFundsSystem);

        private ShareFundsEvents ShareFundsEvents { get; } = new ShareFundsEvents();

        private double _lastFunds;

        protected override bool ShareSystemReady => Funding.Instance != null;

        protected override GameMode RelevantGameModes => GameMode.Career;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnFundsChanged.Add(ShareFundsEvents.FundsChanged);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
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
