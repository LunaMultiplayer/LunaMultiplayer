using LmpClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class PartEvent : LmpBaseEvent
    {
        public static EventData<Part, float> onPartDecoupling;
        public static EventData<Part, float, Vessel> onPartDecoupled;
    }
}
