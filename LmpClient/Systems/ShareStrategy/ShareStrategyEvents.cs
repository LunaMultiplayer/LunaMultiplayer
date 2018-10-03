using System.Linq;
using LmpClient.Base;
using Strategies;

namespace LmpClient.Systems.ShareStrategy
{
    public class ShareStrategyEvents : SubSystem<ShareStrategySystem>
    {
        public void StrategyActivated(Strategy strategy)
        {
            if (System.IgnoreEvents || System.OneTimeStrategies.Contains(strategy.Config.Name)) return;

            LunaLog.Log($"Relaying strategy activation: {strategy.Config.Name} - with factor: {strategy.Factor}");
            System.MessageSender.SendStrategyMessage(strategy);
        }

        public void StrategyDeactivated(Strategy strategy)
        {
            if (System.IgnoreEvents || System.OneTimeStrategies.Contains(strategy.Config.Name)) return;

            LunaLog.Log($"Relaying strategy deactivation: {strategy.Config.Name} - with factor: {strategy.Factor}");
            System.MessageSender.SendStrategyMessage(strategy);
        }
    }
}
