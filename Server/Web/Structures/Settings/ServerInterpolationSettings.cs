using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerInterpolationSettings
    {
        public bool ForceInterpolationOffset => InterpolationSettings.SettingsStore.ForceInterpolationOffset;
        public int InterpolationOffsetMs => InterpolationSettings.SettingsStore.InterpolationOffsetMs;
        public bool ForceInterpolation => InterpolationSettings.SettingsStore.ForceInterpolation;
        public bool InterpolationValue => InterpolationSettings.SettingsStore.InterpolationValue;
        public bool ForceExtrapolation => InterpolationSettings.SettingsStore.ForceExtrapolation;
        public bool ExtrapolationValue => InterpolationSettings.SettingsStore.ExtrapolationValue;
    }
}
