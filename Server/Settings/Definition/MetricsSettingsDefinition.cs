using LmpCommon.Enums;
using LmpCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class MetricsSettingsDefinition
    {
        [XmlComment(Value = "Whether or not to enable the Prometheus metrics endpoint.")]
        public bool Enabled { get; set; } = true;

        [XmlComment(Value = "Whether or not to include the default Prometheus metrics.")]
        public bool EnableDefaultMetrics { get; set; } = false;

        [XmlComment(Value = "Whether or not to include detailed per-player metrics.")]
        public bool EnablePlayerDetailedMetrics { get; set; } = true;

        [XmlComment(Value = "The endpoint to serve the Prometheus metrics on.")]
        public string Endpoint { get; set; } = "metrics";
    }
}
