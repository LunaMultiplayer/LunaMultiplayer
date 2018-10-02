using LmpClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class RevertEvent : LmpBaseEvent
    {
        public static EventVoid onRevertingToLaunch;
        public static EventVoid onRevertedToLaunch;
        public static EventData<EditorFacility> onReturningToEditor;
        public static EventData<EditorFacility> onReturnedToEditor;
    }
}
