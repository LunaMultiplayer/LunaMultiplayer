using LmpClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class WarpEvent : LmpBaseEvent
    {
        public static EventVoid onTimeWarpStopped;
    }
}
