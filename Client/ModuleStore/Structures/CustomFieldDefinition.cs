using LunaCommon.Xml;

namespace LunaClient.ModuleStore.Structures
{
    public class CustomFieldDefinition
    {
        [XmlComment(Value = "Name of the field that we are customizing")]
        public string FieldName { get; set; }

        [XmlComment(Value = "Ignore changes on this field")]
        public bool Ignore { get; set; }

        [XmlComment(Value = "In case we are not ignoring, minimum interval when checking for changes")]
        public int Interval { get; set; }
    }
}
