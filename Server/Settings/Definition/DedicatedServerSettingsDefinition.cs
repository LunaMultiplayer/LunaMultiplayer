using LmpCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class DedicatedServerSettingsDefinition
    {
        [XmlComment(Value = "Use rainbow effect for the server in the server browser")]
        public bool UseRainbowEffect { get; set; } = true;

        [XmlComment(Value = "If UseRainbowEffect is false use RedGreenBlue fields to specify server color (Valid values are from 0 to 255)")]
        public byte Red { get; set; } = 255;

        [XmlComment(Value = "If UseRainbowEffect is false use RedGreenBlue fields to specify server color (Valid values are from 0 to 255)")]
        public byte Green { get; set; } = 0;

        [XmlComment(Value = "If UseRainbowEffect is false use RedGreenBlue fields to specify server color (Valid values are from 0 to 255)")]
        public byte Blue { get; set; } = 0;
    }
}
