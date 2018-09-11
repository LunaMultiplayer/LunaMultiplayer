using LunaCommon.Xml;
using System.Collections.Generic;

namespace LunaClient.ModuleStore.Structures
{
    public class ModuleDefinition
    {
        [XmlComment(Value = "Module that we are modifying")]
        public string ModuleName { get; set; }

        [XmlComment(Value = "Customized methods to sync")]
        public List<MethodDefinition> SyncMethods { get; set; } = new List<MethodDefinition>();
    }
}
