using System.Collections.Generic;
using System.Xml.Serialization;
using LmpClient.Extensions;
using LmpCommon.Xml;
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

        [XmlIgnore]
        public Dictionary<string, FieldDefinition> CustomizedFields { get; private set; } = new Dictionary<string, FieldDefinition>();

        [XmlIgnore]
        public Dictionary<string, MethodDefinition> CustomizedMethods { get; private set; } = new Dictionary<string, MethodDefinition>();

        public void Init()
        {
            CustomizedFields = Fields.DistinctBy(f => f.FieldName).ToDictionary(f => f.FieldName, f => f);
            CustomizedMethods = Methods.DistinctBy(m => m.MethodName).ToDictionary(f => f.MethodName, f => f);
        }

        public void MergeWith(ModuleDefinition other)
        {
            Fields.AddRange(other.Fields);
            Methods.AddRange(other.Methods);
            Init();
        }
    }
}
