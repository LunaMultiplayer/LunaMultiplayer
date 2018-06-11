// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public static class ExternalSeatEvent
    {
        public static EventData<KerbalSeat, KerbalEVA> onExternalSeatBoard { get; } = new EventData<KerbalSeat, KerbalEVA>("onExternalSeatBoard");
        public static EventData<KerbalSeat, KerbalEVA> onExternalSeatUnboard { get; } = new EventData<KerbalSeat, KerbalEVA>("onExternalSeatUnboard");
    }
}
