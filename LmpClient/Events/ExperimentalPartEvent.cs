using LmpClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class ExperimentalPartEvent : LmpBaseEvent
    {
        public static EventData<AvailablePart, int> onExperimentalPartAdded;
        public static EventData<AvailablePart, int> onExperimentalPartRemoved;
    }
}
