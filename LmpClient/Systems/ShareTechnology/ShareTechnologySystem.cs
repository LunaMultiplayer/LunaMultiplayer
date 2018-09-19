using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;

namespace LmpClient.Systems.ShareTechnology
{
    public class ShareTechnologySystem : ShareProgressBaseSystem<ShareTechnologySystem, ShareTechnologyMessageSender, ShareTechnologyMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareTechnologySystem);

        private ShareTechnologyEvents ShareTechnologyEvents { get; } = new ShareTechnologyEvents();

        protected override bool ShareSystemReady => ResearchAndDevelopment.Instance != null;

        protected override GameMode RelevantGameModes => GameMode.Career | GameMode.Science;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            GameEvents.OnTechnologyResearched.Add(ShareTechnologyEvents.TechnologyResearched);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.OnTechnologyResearched.Remove(ShareTechnologyEvents.TechnologyResearched);
        }
    }
}
