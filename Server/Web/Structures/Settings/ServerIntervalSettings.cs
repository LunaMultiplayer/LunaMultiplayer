using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerIntervalSettings
    {
        public int VesselUpdatesMsInterval => IntervalSettings.SettingsStore.VesselUpdatesMsInterval;
        public int SecondaryVesselUpdatesMsInterval => IntervalSettings.SettingsStore.SecondaryVesselUpdatesMsInterval;
        public int SendReceiveThreadTickMs => IntervalSettings.SettingsStore.SendReceiveThreadTickMs;
        public int MainTimeTick => IntervalSettings.SettingsStore.MainTimeTick;
        public int BackupIntervalMs => IntervalSettings.SettingsStore.BackupIntervalMs;
    }
}
