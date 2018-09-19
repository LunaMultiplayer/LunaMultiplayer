using LmpClient.Events;
using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;
using Strategies;
using UnityEngine;

namespace LmpClient.Systems.ShareStrategy
{
    public class ShareStrategySystem : ShareProgressBaseSystem<ShareStrategySystem, ShareStrategyMessageSender, ShareStrategyMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareStrategySystem);

        private ShareStrategyEvents ShareStrategiesEvents { get; } = new ShareStrategyEvents();

        //BailoutGrand - Exchange funds for reputation; researchIPsellout - Exchange funds for science;
        public readonly string[] OneTimeStrategies = { "BailoutGrant", "researchIPsellout" };

        protected override bool ShareSystemReady => StrategySystem.Instance != null && StrategySystem.Instance.Strategies.Count != 0 && Funding.Instance != null && ResearchAndDevelopment.Instance != null &&
                                                    Reputation.Instance != null && Time.timeSinceLevelLoad > 1f;

        protected override GameMode RelevantGameModes => GameMode.Career;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;
            StrategyEvent.onStrategyActivated.Add(ShareStrategiesEvents.StrategyActivated);
            StrategyEvent.onStrategyDeactivated.Add(ShareStrategiesEvents.StrategyDeactivated);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            StrategyEvent.onStrategyActivated.Remove(ShareStrategiesEvents.StrategyActivated);
            StrategyEvent.onStrategyDeactivated.Remove(ShareStrategiesEvents.StrategyDeactivated);
        }
    }
}
