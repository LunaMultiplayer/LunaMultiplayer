using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class ConnectionSettingsDefinition
    {
        [XmlComment(Value = "The UDP port the server listens on. You don't need to open it on your router if RegisterWithMasterServer = true. " +
                            "If you want that players can connect against your server MANUALLY you will need to open it on your router")]
        public int Port { get; set; } = 8800;

        [XmlComment(Value = "Heartbeat interval in Ms. MUST be lower than the ConnectionMsTimeout value.")]
        public int HearbeatMsInterval { get; set; } = 1000;

        [XmlComment(Value = "Connection timeout in Ms. If no heartbeats are received after this interval, the client is disconnected.")]
        public int ConnectionMsTimeout { get; set; } = 30000;
    }
}
