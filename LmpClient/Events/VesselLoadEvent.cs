using LmpClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class VesselLoadEvent : LmpBaseEvent
    {
        public static EventData<Vessel> onLmpVesselLoaded;
    }
}
