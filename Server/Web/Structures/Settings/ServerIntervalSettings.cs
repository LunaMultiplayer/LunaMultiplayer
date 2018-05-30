using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerIntervalSettings
    {
        public int VesselPositionUpdatesMsInterval => IntervalSettings.SettingsStore.VesselPositionUpdatesMsInterval;
        public int SecondaryVesselPositionUpdatesMsInterval => IntervalSettings.SettingsStore.SecondaryVesselPositionUpdatesMsInterval;
        public int VesselPartsSyncMsInterval => IntervalSettings.SettingsStore.VesselPartsSyncMsInterval;
        public int SendReceiveThreadTickMs => IntervalSettings.SettingsStore.SendReceiveThreadTickMs;
        public int MainTimeTick => IntervalSettings.SettingsStore.MainTimeTick;
        public int BackupIntervalMs => IntervalSettings.SettingsStore.BackupIntervalMs;
    }
}
