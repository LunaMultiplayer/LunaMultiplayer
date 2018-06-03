// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class WarpEvent
    {
        public static EventVoid onTimeWarpStopped { get; } = new EventVoid("onTimeWarpStopped");
        public static EventVoid onTimeWarpStarted { get; } = new EventVoid("onTimeWarpStarted");
    }
}
