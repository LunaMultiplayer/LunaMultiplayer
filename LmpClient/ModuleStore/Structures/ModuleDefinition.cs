using LmpCommon.Xml;
using System.Collections.Generic;
using UniLinq;

namespace LmpClient.ModuleStore.Structures
{
    public class ModuleDefinition
    {
        [XmlComment(Value = "Module that we are modifying")]
        public string ModuleName { get; set; }

        [XmlComment(Value = "Fields to sync")]
        public List<FieldDefinition> Fields { get; set; } = new List<FieldDefinition>();
        
        [XmlComment(Value = "Methods to sync")]
        public List<MethodDefinition> Methods { get; set; } = new List<MethodDefinition>();

        public FieldDefinition GetCustomizationForField(string fieldName) => Fields.FirstOrDefault(f => f.FieldName == fieldName);

        public MethodDefinition GetCustomizationForMethod(string methodName) => Methods.FirstOrDefault(f => f.MethodName == methodName);
    }
}
