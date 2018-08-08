using LunaCommon.Enums;
using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class WarpSettingsDefinition
    {
        [XmlComment(Value = "Specify the warp Type. Values: None, Subspace")]
        public WarpMode WarpMode { get; set; } = WarpMode.Subspace;

        [XmlComment(Value = "Username of the player who control the warp if WarpMode is set to MASTER")]
        public string WarpMaster { get; set; } = "";
    }
}
