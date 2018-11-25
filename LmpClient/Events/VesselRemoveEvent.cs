using LmpClient.Events.Base;
using System;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class VesselRemoveEvent : LmpBaseEvent
    {
        public static EventData<Guid> onLmpVesselRemoved;
    }
}
