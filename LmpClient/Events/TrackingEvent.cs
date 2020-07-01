using LmpClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class TrackingEvent : LmpBaseEvent
    {
        public static EventData<Vessel> onStartTrackingAsteroidOrComet;
        public static EventData<Vessel> onStopTrackingAsteroidOrComet;
    }
}
