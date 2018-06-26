using LunaClient.Events.Base;
using System;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class ExternalSeatEvent : LmpBaseEvent
    {
        public static EventData<KerbalSeat, Guid, string> onExternalSeatBoard;
        public static EventData<Vessel, KerbalEVA> onExternalSeatUnboard;
    }
}
