// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public static class ImmortalEvent
    {
        public static EventData<Vessel> onVesselImmortal { get; } = new EventData<Vessel>("onVesselImmortal");
        public static EventData<Vessel> onVesselMortal { get; } = new EventData<Vessel>("onVesselMortal");
    }
}
