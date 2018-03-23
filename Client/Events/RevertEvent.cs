// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class RevertEvent
    {
        public static EventVoid onRevertToLaunch { get; } = new EventVoid("onRevertToLaunch");
        public static EventData<EditorFacility> onRevertToPrelaunch { get; } = new EventData<EditorFacility>("onRevertToPrelaunch");
        public static EventData<EditorFacility> onReturnToEditor { get; } = new EventData<EditorFacility>("onReturnToEditor");
    }
}
