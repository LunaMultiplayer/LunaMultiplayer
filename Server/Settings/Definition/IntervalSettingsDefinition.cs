using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class IntervalSettingsDefinition
    {
        [XmlComment(Value = "Interval in ms at which the client will send POSITION and FLIGHTSTATE updates of his vessel when other players are NEARBY. " +
                "Decrease it if your clients have good network connection and you plan to do dogfights, although in that case consider using interpolation aswell")]
        public int VesselUpdatesMsInterval { get; set; } = 50;

        [XmlComment(Value = "Interval in ms at which the client will send POSITION and FLIGHTSTATE updates for vessels that are uncontrolled and nearby him. " +
                            "This interval is also applied used to send position updates of HIS OWN vessel when NOBODY is around")]
        public int SecondaryVesselUpdatesMsInterval { get; set; } = 150;

        [XmlComment(Value = "Send/Receive tick clock. Keep this value low but at least above 2ms to avoid extreme CPU usage.")]
        public int SendReceiveThreadTickMs { get; set; } = 5;

        [XmlComment(Value = "Main thread polling in ms. Keep this value low but at least above 2ms to avoid extreme CPU usage.")]
        public int MainTimeTick { get; set; } = 5;

        [XmlComment(Value = "Interval in ms at which internal LMP structures (Subspaces, Vessels, Scenario files, ...) will be backed up to a file")]
        public int BackupIntervalMs { get; set; } = 30000;
    }
}
