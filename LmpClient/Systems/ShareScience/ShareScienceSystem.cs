using LmpClient.Events;
using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;

namespace LmpClient.Systems.ShareScience
{
    public class ShareScienceSystem : ShareProgressBaseSystem<ShareScienceSystem, ShareScienceMessageSender, ShareScienceMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareScienceSystem);

        private ShareScienceEvents ShareScienceEvents { get; } = new ShareScienceEvents();

        private float _lastScience;

        protected override bool ShareSystemReady => ResearchAndDevelopment.Instance != null;

        protected override GameMode RelevantGameModes => GameMode.Career | GameMode.Science;

        public bool Reverting { get; set; }

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnScienceChanged.Add(ShareScienceEvents.ScienceChanged);

            RevertEvent.onRevertingToLaunch.Add(ShareScienceEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Add(ShareScienceEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Add(ShareScienceEvents.LevelLoaded);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnScienceChanged.Remove(ShareScienceEvents.ScienceChanged);

            RevertEvent.onRevertingToLaunch.Remove(ShareScienceEvents.RevertingDetected);
            RevertEvent.onReturningToEditor.Remove(ShareScienceEvents.RevertingToEditorDetected);
            GameEvents.onLevelWasLoadedGUIReady.Remove(ShareScienceEvents.LevelLoaded);

            Reverting = false;
            _lastScience = 0;
        }

        public override void SaveState()
        {
            base.SaveState();
            _lastScience = ResearchAndDevelopment.Instance.Science;
        }

        public override void RestoreState()
        {
            base.RestoreState();
            ResearchAndDevelopment.Instance.SetScience(_lastScience, TransactionReasons.None);
        }

        public void SetScienceWithoutTriggeringEvent(float science)
        {
            if (!CurrentGameModeIsRelevant) return;

            StartIgnoringEvents();
            ResearchAndDevelopment.Instance.SetScience(science, TransactionReasons.None);
            StopIgnoringEvents();
        }
    }
}
