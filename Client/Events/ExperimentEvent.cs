// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public static class ExperimentEvent
    {
        public static EventData<Vessel> onExperimentReset { get; } = new EventData<Vessel>("onExperimentReset");
    }
}
