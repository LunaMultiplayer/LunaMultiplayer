using LmpCommon.Xml;

namespace LmpClient.ModuleStore.Structures
{
    public class FieldDefinition
    {
        [XmlComment(Value = "Name of the field that we are customizing")]
        public string FieldName { get; set; }

        [XmlComment(Value = "Max interval to sync this field")]
        public int MaxIntervalInMs { get; set; }
    }
}
