using LunaClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class PartModuleEvent : LmpBaseEvent
    {
        public static EventData<PartModule, string> onPartModuleFieldChange;
    }
}
