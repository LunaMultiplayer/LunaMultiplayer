using LunaCommon.Xml;
using System.Collections.Generic;

namespace LunaClient.ModuleStore.Structures
{
    public class MethodDefinition
    {
        [XmlComment(Value = "Name of the method that we are customizing")]
        public string MethodName { get; set; }

        [XmlComment(Value = "Max interval to sync this method call")]
        public int MaxIntervalInMs { get; set; }

        [XmlComment(Value = "Fields to sync after the method is called")]
        public List<string> Fields { get; set; } = new List<string>();
    }
}
