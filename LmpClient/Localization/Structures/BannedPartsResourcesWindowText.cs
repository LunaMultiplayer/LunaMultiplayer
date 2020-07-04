namespace LmpClient.Localization.Structures
{
    public class BannedPartsResourcesWindowText
    {
        public string Title { get; set; } = "LunaMultiplayer - Banned parts/resources";
        public string Text { get; set; } = "contains the following banned parts/resources and cannot be launched:";
        public string BannedParts { get; set; } = "Banned parts:";
        public string BannedResources { get; set; } = "Banned resources:";
        public string TooManyParts { get; set; } = "Your vessel has too many parts. This server allows vessels with a max part count of:";
    }
}
