using LmpCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class MasterServerSettingsDefinition
    {
        [XmlComment(Value = "Set to false if you don't want to appear on the server list")]
        public bool RegisterWithMasterServer { get; set; } = true;

        [XmlComment(Value = "Specify in miliseconds how often we will update the info with masterserver. Min value = 5000")]
        public int MasterServerRegistrationMsInterval { get; set; } = 5000;
    }
}
