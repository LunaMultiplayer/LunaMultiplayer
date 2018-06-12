using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerDebugSettings
    {
        public float SimulatedLossChance => DebugSettings.SettingsStore.SimulatedLossChance;
        public float SimulatedDuplicatesChance => DebugSettings.SettingsStore.SimulatedDuplicatesChance;
        public int MaxSimulatedRandomLatencyMs => DebugSettings.SettingsStore.MaxSimulatedRandomLatencyMs;
        public int MinSimulatedLatencyMs => DebugSettings.SettingsStore.MinSimulatedLatencyMs;
    }
}
