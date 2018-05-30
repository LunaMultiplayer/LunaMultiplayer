using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerDebugSettings
    {
        public int SimulatedLossChance => DebugSettings.SettingsStore.SimulatedLossChance;
        public int SimulatedDuplicatesChance => DebugSettings.SettingsStore.SimulatedDuplicatesChance;
        public int MaxSimulatedRandomLatencyMs => DebugSettings.SettingsStore.MaxSimulatedRandomLatencyMs;
        public int MinSimulatedLatencyMs => DebugSettings.SettingsStore.MinSimulatedLatencyMs;
        public int SimulatedMsTimeOffset => DebugSettings.SettingsStore.SimulatedMsTimeOffset;
    }
}
