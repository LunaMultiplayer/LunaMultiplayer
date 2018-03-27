using LunaClient.Base;
using Strategies;

namespace LunaClient.Systems.ShareStrategy
{
    public class ShareStrategyEvents : SubSystem<ShareStrategySystem>
    {
        public void StrategyActivated(Strategy strategy)
        {
            if (System.IgnoreEvents) return;

            LunaLog.Log($"Relaying strategy activation: {strategy.Title} - with factor: {strategy.Factor}");
            System.MessageSender.SendStrategyMessage(strategy);
        }

        public void StrategyDeactivated(Strategy strategy)
        {
            if (System.IgnoreEvents) return;

            LunaLog.Log($"Relaying strategy deactivation: {strategy.Title} - with factor: {strategy.Factor}");
            System.MessageSender.SendStrategyMessage(strategy);
        }
    }
}
