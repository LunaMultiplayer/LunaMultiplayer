// ReSharper disable All

using System;

#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public static class ExternalSeatEvent
    {
        public static EventData<KerbalSeat, Guid, string> onExternalSeatBoard { get; } = new EventData<KerbalSeat, Guid, string>("onExternalSeatBoard");
        public static EventData<KerbalSeat, KerbalEVA> onExternalSeatUnboard { get; } = new EventData<KerbalSeat, KerbalEVA>("onExternalSeatUnboard");
    }
}
