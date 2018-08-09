using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareScience
{
    public class ShareScienceSystem : ShareProgressBaseSystem<ShareScienceSystem, ShareScienceMessageSender, ShareScienceMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareScienceSystem);

        private ShareScienceEvents ShareScienceEvents { get; } = new ShareScienceEvents();

        private float _lastScience;

        protected override bool ShareSystemReady => ResearchAndDevelopment.Instance != null;

        protected override GameMode RelevantGameModes => GameMode.Career | GameMode.Science;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnScienceChanged.Add(ShareScienceEvents.ScienceChanged);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnScienceChanged.Remove(ShareScienceEvents.ScienceChanged);
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
    }
}
