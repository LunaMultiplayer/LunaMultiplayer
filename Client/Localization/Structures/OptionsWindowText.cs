namespace LunaClient.Localization.Structures
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
        public string Interpolation { get; set; } = "Enable interpolation";
        public string GenerateLmpModControl { get; set; } = "Generate a server LMPModControl.xml";
        public string GenerateUniverse { get; set; } = "Generate Universe from saved game";
        public string NetworkSettings { get; set; } = "Network settings";
        public string CannotChangeWhileConnected { get; set; } = "Cannot change values while connected";
        public string ResetNetwork { get; set; } = "Reset network";
    }
}
