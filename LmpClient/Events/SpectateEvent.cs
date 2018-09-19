using LmpClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class SpectateEvent : LmpBaseEvent
    {
        public static EventVoid onStartSpectating;
        public static EventVoid onFinishedSpectating;
    }
}
