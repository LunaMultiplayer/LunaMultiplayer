using LunaClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class WarpEvent : LmpBaseEvent
    {
        public static EventVoid onTimeWarpStopped;
    }
}
