using LunaCommon.Xml;
using System.Collections.Generic;

namespace LunaClient.ModuleStore.Structures
{
    public class ModuleDefinition
    {
        [XmlComment(Value = "Module that we are modifying")]
        public string ModuleName { get; set; }

        [XmlComment(Value = "Customized fields for the specified module")]
        public List<FieldDefinition> Fields { get; set; } = new List<FieldDefinition>();
    }
}
