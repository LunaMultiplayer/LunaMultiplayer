// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class SpectateEvent
    {
        public static EventVoid onStartSpectating { get; } = new EventVoid("onStartSpectating");
        public static EventVoid onFinishedSpectating { get; } = new EventVoid("onFinishedSpectating");
    }
}
