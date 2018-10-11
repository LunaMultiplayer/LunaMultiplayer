using LmpClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class RailEvent : LmpBaseEvent
    {
        public static EventData<Vessel> onVesselGoneOffRails;
        public static EventData<Vessel> onVesselGoneOnRails;
    }
}
