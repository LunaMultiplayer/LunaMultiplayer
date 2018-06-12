namespace LunaClient.Systems.SettingsSys
{
    public static class SettingsBackup
    {
        private static double? InterpolationOffsetSeconds { get; set; }
        private static bool? PositionInterpolation { get; set; }
        private static bool? PositionExtrapolation { get; set; }

        public static void CreateBackup()
        {
            if (SettingsSystem.ServerSettings.ForceInterpolationOffset)
                InterpolationOffsetSeconds = SettingsSystem.CurrentSettings.InterpolationOffsetSeconds;
            else
                InterpolationOffsetSeconds = null;

            if (SettingsSystem.ServerSettings.ForceInterpolation)
                PositionInterpolation = SettingsSystem.CurrentSettings.PositionInterpolation;
            else
                PositionInterpolation = null;

            if (SettingsSystem.ServerSettings.ForceExtrapolation)
                PositionExtrapolation = SettingsSystem.CurrentSettings.PositionExtrapolation;
            else
                PositionExtrapolation = null;
        }

        public static void RestoreBackup()
        {
            if (InterpolationOffsetSeconds.HasValue) SettingsSystem.CurrentSettings.InterpolationOffsetSeconds = InterpolationOffsetSeconds.Value;
            if (PositionInterpolation.HasValue) SettingsSystem.CurrentSettings.PositionInterpolation = PositionInterpolation.Value;
            if (PositionExtrapolation.HasValue) SettingsSystem.CurrentSettings.PositionExtrapolation = PositionExtrapolation.Value;

            SettingsSystem.SaveSettings();
        }
    }
}
