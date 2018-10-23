using Lidgren.Network;

namespace LmpClient.Localization.Structures
{
    public class OptionsWindowText
    {
        public string Title { get; set; } = "Options";
        public string Language { get; set; } = "Language:";
        public string Color { get; set; } = "Player color:";
        public string Red { get; set; } = "R:";
        public string Green { get; set; } = "G:";
        public string Blue { get; set; } = "B:";
        public string Random { get; set; } = "Random";
        public string Set { get; set; } = "Set";
        public string InterpolationSettings { get; set; } = "Interpolation settings";
        public string EnableInterpolation { get; set; } = "Enable interpolation";
        public string InterpolationOffset { get; set; } = "Interpolation offset:";
        public string GenerateLmpModControl { get; set; } = "Generate a server LMPModControl.xml";
        public string GenerateUniverse { get; set; } = "Generate Universe from saved game";
        public string NetworkSettings { get; set; } = "Network settings";
        public string CannotChangeWhileConnected { get; set; } = "Cannot change values while connected";
        public string ResetNetwork { get; set; } = "Reset network";
        public string Mtu { get; set; } = $"MTU (Default: {NetPeerConfiguration.kDefaultMTU}):";
        public string AutoExpandMtu { get; set; } = "Auto expand MTU";
        public string ConnectionTimeout { get; set; } = "Connection timeout (Default: 15):";
        public string GeneralSettings { get; set; } = "General settings";
        public string IgnoreSyncChecks { get; set; } = "Ignore warp sync safety checks";
    }
}
