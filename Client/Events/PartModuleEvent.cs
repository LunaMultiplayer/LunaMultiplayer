// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public static class PartModuleEvent
    {
        public static EventData<PartModule, string> onPartModuleFieldChange { get; } = new EventData<PartModule, string>("onPartModuleFieldChange");
    }
}
