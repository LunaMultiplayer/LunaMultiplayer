using LmpCommon.Xml;
using Server.Log;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class LogSettingsDefinition
    {
        [XmlComment(Value = "Minimum log level. Values: Normal, Debug, NetworkDebug, VerboseNetworkDebug")]
        public LogLevels LogLevel { get; set; } = LogLevels.Debug;

        [XmlComment(Value = "Specify the amount of days a log file should be considered as expired and deleted. 0 = Disabled")]
        public int ExpireLogs { get; set; } = 14;

        [XmlComment(Value = "Use UTC instead of system time in the log.")]
        public bool UseUtcTimeInLog { get; set; } = false;
    }
}
