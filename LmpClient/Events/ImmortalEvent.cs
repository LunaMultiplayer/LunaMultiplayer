using LmpClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class ImmortalEvent : LmpBaseEvent
    {
        public static EventData<Vessel> onVesselImmortal;
        public static EventData<Vessel> onVesselMortal;
    }
}
