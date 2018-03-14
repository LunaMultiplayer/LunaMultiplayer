namespace LunaClient.Localization.Structures
{
    public class ModWindowText
    {
        public string Title { get; set; } = "LunaMultiplayer - Failed Mod Validation";
        public string Close { get; set; } = "Close";
        public string MandatoryModsNotFound { get; set; } = "Mandatory mods not found:";
        public string MissingExpansions { get; set; } = "Missing expansions:";
        public string MandatoryModsDifferentShaFound { get; set; } = "Mandatory mods with different SHA found:";
        public string Link { get; set; } = "Link";
        public string ForbiddenFilesFound { get; set; } = "Forbidden mods found:";
        public string NonListedFilesFound { get; set; } = "Non listed and forbidden mods found:";
        public string MandatoryPartsNotFound { get; set; } = "Mandatory parts not found:";
        public string ForbiddenPartsFound { get; set; } = "Forbidden parts found:";
    }
}
