using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class ScreenshotSettingsDefinition
    {
        [XmlComment(Value = "Minimum interval between screenshots in milliseconds")]
        public int MinScreenshotIntervalMs { get; set; } = 30000;

        [XmlComment(Value = "Maximum screenshots kept per user")]
        public int MaxScreenshotsPerUser { get; set; } = 30;

        [XmlComment(Value = "Maximum screenshots folders kept")]
        public int MaxScreenshotsFolders { get; set; } = 50;

    }
}
