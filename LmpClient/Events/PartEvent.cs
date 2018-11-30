using LmpClient.Events.Base;
using System;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class PartEvent : LmpBaseEvent
    {
        public static EventData<Part, float> onPartDecoupling;
        public static EventData<Part, float, Vessel> onPartDecoupled;

        public static EventData<Part, Part> onPartCoupling;
        public static EventData<Part, Part, Guid> onPartCoupled;
    }
}
