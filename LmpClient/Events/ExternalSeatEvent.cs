using LmpClient.Events.Base;
using System;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class ExternalSeatEvent : LmpBaseEvent
    {
        public static EventData<KerbalSeat, Guid, uint, string> onExternalSeatBoard;
        public static EventData<Vessel, KerbalEVA> onExternalSeatUnboard;
    }
}
