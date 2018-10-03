using LmpClient.Events.Base;
using Strategies;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class StrategyEvent : LmpBaseEvent
    {
        public static EventData<Strategy> onStrategyActivated;
        public static EventData<Strategy> onStrategyDeactivated;
    }
}
