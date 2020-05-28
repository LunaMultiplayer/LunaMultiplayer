using LmpClient.Events;
using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;
using System;
using Guid = System.Guid;

namespace LmpClient.Systems.ShareFunds
{
    public class ShareFundsSystem : ShareProgressBaseSystem<ShareFundsSystem, ShareFundsMessageSender, ShareFundsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareFundsSystem);

        private ShareFundsEvents ShareFundsEvents { get; } = new ShareFundsEvents();

        private double _lastFunds;

        //This queue system is not used because we use one big queue in ShareCareerSystem for this system.
        protected override bool ShareSystemReady => true;

        protected override GameMode RelevantGameModes => GameMode.Career;

        public bool Reverting { get; set; }

        public Tuple<Guid, float> CurrentShipCost { get; set; }

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnFundsChanged.Add(ShareFundsEvents.FundsChanged);

            RevertEvent.onRevertingToLaunch.Add(ShareFundsEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Add(ShareFundsEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Add(ShareFundsEvents.LevelLoaded);
            GameEvents.onVesselSwitching.Add(ShareFundsEvents.VesselSwitching);
            VesselAssemblyEvent.onAssembledVessel.Add(ShareFundsEvents.VesselAssembled);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnFundsChanged.Remove(ShareFundsEvents.FundsChanged);

            RevertEvent.onRevertingToLaunch.Remove(ShareFundsEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Remove(ShareFundsEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Remove(ShareFundsEvents.LevelLoaded);
            GameEvents.onVesselSwitching.Remove(ShareFundsEvents.VesselSwitching);
            VesselAssemblyEvent.onAssembledVessel.Remove(ShareFundsEvents.VesselAssembled);

            _lastFunds = 0;
            Reverting = false;
        }

        public override void SaveState()
        {
            base.SaveState();
            _lastFunds = Funding.Instance.Funds;
        }

        public override void RestoreState()
        {
            base.RestoreState();
            Funding.Instance.SetFunds(_lastFunds, TransactionReasons.None);
        }

        public void SetFundsWithoutTriggeringEvent(double funds)
        {
            if (!CurrentGameModeIsRelevant) return;

            StartIgnoringEvents();
            Funding.Instance.SetFunds(funds, TransactionReasons.None);
            StopIgnoringEvents();
        }
    }
}
