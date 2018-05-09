using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class CraftSettingsDefinition
    {
        [XmlComment(Value = "Minimum interval between uploading/requesting crafts in milliseconds")]
        public int MinCraftLibraryRequestIntervalMs { get; set; } = 5000;

        [XmlComment(Value = "Maximum crafts kept per user per type (VAB,SPH and Subassembly)")]
        public int MaxCraftsPerUser { get; set; } = 10;

        [XmlComment(Value = "Maximum crafts folders kept")]
        public int MaxCraftFolders { get; set; } = 50;
    }
}
