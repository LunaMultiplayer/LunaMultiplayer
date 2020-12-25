using LmpClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class EVAConstructionEvent : LmpBaseEvent
    {
        public static EventData<Part> onAttachingPart;
        public static EventVoid onDroppingPart;
        public static EventVoid onDroppedPart;
    }
}
