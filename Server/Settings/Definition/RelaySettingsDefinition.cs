using LunaCommon.Xml;
using Server.Enums;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class RelaySettingsDefinition
    {
        [XmlComment(Value = "Relay system mode. Dictionary uses more RAM but it's faster. Database use disk space instead but it's slower. Values: Dictionary, Database")]
        public RelaySystemMode RelaySystemMode { get; set; } = RelaySystemMode.Dictionary;

        [XmlComment(Value = "Interval for saving POSITION updates IN THE SERVER so they are later sent to the OTHER players in the past. " +
                            "Lower number => smoother movement but as you're saving more position updates, then more memory will be required")]
        public int RelaySaveIntervalMs { get; set; } = 1000;
    }
}
