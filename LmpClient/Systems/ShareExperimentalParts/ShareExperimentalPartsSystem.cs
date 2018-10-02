using LmpClient.Events;
using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;

namespace LmpClient.Systems.ShareExperimentalParts
{
    public class ShareExperimentalPartsSystem : ShareProgressBaseSystem<ShareExperimentalPartsSystem, ShareExperimentalPartsMessageSender, ShareExperimentalPartsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareExperimentalPartsSystem);

        private ShareExperimentalPartsEvents ShareExperimentalPartsEvents { get; } = new ShareExperimentalPartsEvents();

        protected override bool ShareSystemReady => ResearchAndDevelopment.Instance != null;

        protected override GameMode RelevantGameModes => GameMode.Career;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;

            ExperimentalPartEvent.onExperimentalPartRemoved.Add(ShareExperimentalPartsEvents.ExperimentalPartRemoved);
            ExperimentalPartEvent.onExperimentalPartAdded.Add(ShareExperimentalPartsEvents.ExperimentalPartAdded);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            ExperimentalPartEvent.onExperimentalPartRemoved.Remove(ShareExperimentalPartsEvents.ExperimentalPartRemoved);
            ExperimentalPartEvent.onExperimentalPartAdded.Remove(ShareExperimentalPartsEvents.ExperimentalPartAdded);
        }
    }
}
