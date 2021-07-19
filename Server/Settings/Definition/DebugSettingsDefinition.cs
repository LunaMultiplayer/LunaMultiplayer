using LmpCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class DebugSettingsDefinition
    {
        [XmlComment(Value = "A percentage value to simulate the probability of a packet loss. From 0.0% to 100.0%")]
        public float SimulatedLossChance { get; set; } = 0;

        [XmlComment(Value = "A percentage value to simulate the probability of a packet duplication. From 0.0% to 100.0%")]
        public float SimulatedDuplicatesChance { get; set; } = 0;

        [XmlComment(Value = "Max random latency that a packet may have")]
        public int MaxSimulatedRandomLatencyMs { get; set; } = 0;

        [XmlComment(Value = "Minimum latency that a packet will have")]
        public int MinSimulatedLatencyMs { get; set; } = 0;

        [XmlComment(Value = "Custom master server to register with for debugging purposes")]
        public string CustomMasterServer { get; set; } = "";
    }
}
