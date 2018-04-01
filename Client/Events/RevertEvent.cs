// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class RevertEvent
    {
        public static EventVoid onRevertingToLaunch { get; } = new EventVoid("onRevertingToLaunch");
        public static EventVoid onRevertedToLaunch { get; } = new EventVoid("onRevertedToLaunch");
        public static EventData<EditorFacility> onRevertingToPrelaunch { get; } = new EventData<EditorFacility>("onRevertingToPrelaunch");
        public static EventData<EditorFacility> onRevertedToPrelaunch { get; } = new EventData<EditorFacility>("onRevertedToPrelaunch");
        public static EventData<EditorFacility> onReturningToEditor { get; } = new EventData<EditorFacility>("onReturningToEditor");
        public static EventData<EditorFacility> onReturnedToEditor { get; } = new EventData<EditorFacility>("onReturnedToEditor");
    }
}
