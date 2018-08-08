using LunaClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class SafetyBubbleEvent : LmpBaseEvent
    {
        public static EventData<Vector3d> onEnteringSafetyBubble;
        public static EventVoid onLeavingSafetyBubble;
    }
}
