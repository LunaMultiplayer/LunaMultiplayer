using Strategies;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class StrategyEvent
    {
        public static EventData<Strategy> onStrategyActivated { get; } = new EventData<Strategy>("onStrategyActivated");
        public static EventData<Strategy> onStrategyDeactivated { get; } = new EventData<Strategy>("onStrategyDeactivated");
    }
}
