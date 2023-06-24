using LmpCommon.Enums;
using LmpCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class MetricsSettingsDefinition
    {
        [XmlComment(Value = "Whether or not to enable the Prometheus metrics endpoint.")]
        public bool Enabled { get; set; } = false;

        [XmlComment(Value = "The endpoint to serve the Prometheus metrics on.")]
        public string Endpoint { get; set; } = "metrics";

        [XmlComment(Value = "Whether or not to include detailed per-player metrics.")]
        public bool DetailedPlayerMetrics { get; set; } = false;
    }
}