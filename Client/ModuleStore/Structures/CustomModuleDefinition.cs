using LunaCommon.Xml;
using System.Collections.Generic;

namespace LunaClient.ModuleStore.Structures
{
    public class CustomModuleDefinition
    {
        [XmlComment(Value = "Module that we are modifying")]
        public string ModuleName { get; set; }

        [XmlComment(Value = "Customized fields for the specified module")]
        public List<CustomFieldDefinition> Fields { get; set; } = new List<CustomFieldDefinition>();
    }
}
