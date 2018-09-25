using LmpClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class TrackingEvent : LmpBaseEvent
    {
        public static EventData<Vessel> onStartTrackingAsteroid;
        public static EventData<Vessel> onStopTrackingAsteroid;
    }
}
