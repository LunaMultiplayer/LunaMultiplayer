using LunaClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class PartModuleEvent : LmpBaseEvent
    {
        public static EventData<PartModule, string, KSPActionParam> onPartModuleActionCalled;
        public static EventData<PartModule, string> onPartModuleEventCalled;
        public static EventData<PartModule, string> onPartModuleMethodCalled;
    }
}
