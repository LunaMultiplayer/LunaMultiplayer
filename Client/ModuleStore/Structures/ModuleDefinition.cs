using LunaCommon.Xml;
using System.Collections.Generic;

namespace LunaClient.ModuleStore.Structures
{
    public class ModuleDefinition
    {
        [XmlComment(Value = "Module that we are modifying")]
        public string ModuleName { get; set; }

        [XmlComment(Value = "KSPAction methods to sync")]
        public List<MethodDefinition> ActionMethods { get; set; } = new List<MethodDefinition>();

        [XmlComment(Value = "KSPEvent methods to sync")]
        public List<MethodDefinition> EventMethods { get; set; } = new List<MethodDefinition>();

        [XmlComment(Value = "Standard methods to sync")]
        public List<MethodDefinition> Methods { get; set; } = new List<MethodDefinition>();
    }
}
