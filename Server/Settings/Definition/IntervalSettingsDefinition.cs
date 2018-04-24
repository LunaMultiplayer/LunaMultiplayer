using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class IntervalSettingsDefinition
    {
        [XmlComment(Value = "Interval in Ms at which the client will send POSITION updates of his vessel when other players are NEARBY. " +
                "Decrease it if your clients have good network connection and you plan to do dogfights, although in that case consider using interpolation aswell")]
        public int VesselPositionUpdatesMsInterval { get; set; } = 80;

        [XmlComment(Value = "Interval in Ms at which the client will send POSITION updates for vessels that are uncontrolled and nearby him. " +
                            "This interval is also applied used to send position updates of HIS OWN vessel when NOBODY is around")]
        public int SecondaryVesselPositionUpdatesMsInterval { get; set; } = 500;

        [XmlComment(Value = "Interval in ms at which users will check the controlled and close uncontrolled vessel and sync the parts that have changes " +
                            "(ladders that extend or shields that open) to the server. " +
                            "Caution! Puting a very low value could make clients with slow computers to lag a lot!")]
        public int VesselPartsSyncMsInterval { get; set; } = 500;

        [XmlComment(Value = "Send/Receive tick clock. Keep this value low but at least above 2ms to avoid extreme CPU usage.")]
        public int SendReceiveThreadTickMs { get; set; } = 5;

        [XmlComment(Value = "Main thread polling in ms. Keep this value low but at least above 2ms to avoid extreme CPU usage.")]
        public int MainTimeTick { get; set; } = 5;

        [XmlComment(Value = "Interval in ms at which internal LMP structures (Subspaces, Vessels, Scenario files, ...) will be backed up to a file")]
        public int BackupIntervalMs { get; set; } = 30000;
    }
}
