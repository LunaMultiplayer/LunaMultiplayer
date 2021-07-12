using LmpCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class WebsiteSettingsDefinition
    {
        [XmlComment(Value = "Enable the website system that allows to retrieve server information in JSON format. You can get the data at http://YourIP:Port")]
        public bool EnableWebsite { get; set; } = true;

        [XmlComment(Value = "The address that the web server listens on. Falls back to ConnectionSettings.ListenAddress if unset, which defaults to 0.0.0.0")]
        public string ListenAddress { get; set; } = "";

        [XmlComment(Value = "TCP port. You will need to open this port on your router if you want to display the data from outside your local network")]
        public int Port { get; set; } = 8900;

        [XmlComment(Value = "Interval for refreshing the information of players and vessels")]
        public int RefreshIntervalMs { get; set; } = 5000;
    }
}
