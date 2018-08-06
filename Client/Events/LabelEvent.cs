using LunaClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class LabelEvent : LmpBaseEvent
    {
        public static EventData<BaseLabel> onLabelProcessed;
    }
}
