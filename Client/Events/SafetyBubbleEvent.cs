using LunaClient.Events.Base;
using LunaClient.Systems.SafetyBubbleDrawer;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class SafetyBubbleEvent : LmpBaseEvent
    {
        public static EventData<SpawnPointLocation> onEnteringSafetyBubble;
        public static EventVoid onLeavingSafetyBubble;
    }
}
