// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class SpectateEvent
    {
        public static EventVoid onStartSpectating = new EventVoid("onStartSpectating");
        public static EventVoid onFinishedSpectating = new EventVoid("onFinishedSpectating");
    }
}
