using LunaClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class ImmortalEvent : LmpBaseEvent
    {
        public static EventData<Vessel> onVesselImmortal;
        public static EventData<Vessel> onVesselMortal;
    }
}
