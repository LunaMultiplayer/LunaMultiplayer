using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class WebsiteSettingsDefinition
    {
        [XmlComment(Value = "Enable the website system that allows retrieve of server information. You can get the json data at http://127.0.0.1:Port")]
        public bool EnableWebsite { get; set; } = true;

        [XmlComment(Value = "TCP port. You NEED to open this port on your router so it can be seen from outside your local network!")]
        public int Port { get; set; } = 8900;

        [XmlComment(Value = "Interval for refreshing the information that the server stores")]
        public int RefreshIntervalMs { get; set; } = 5000;
    }
}
