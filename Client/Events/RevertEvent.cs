// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class RevertEvent
    {
        public static EventVoid onRevertToLaunch = new EventVoid("onRevertToLaunch");
        public static EventData<EditorFacility> onRevertToPrelaunch = new EventData<EditorFacility>("onRevertToPrelaunch");
        public static EventData<EditorFacility> onReturnToEditor = new EventData<EditorFacility>("onReturnToEditor");
    }
}
