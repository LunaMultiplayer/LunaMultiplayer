using LunaCommon.Xml;

namespace LunaClient.ModuleStore.Structures
{
    public class FieldDefinition
    {
        [XmlComment(Value = "Name of the field that we are customizing")]
        public string FieldName { get; set; }

        [XmlComment(Value = "Ignore changes on this field")]
        public bool Ignore { get; set; }

        [XmlComment(Value = "Ignore changes on this field while spectating")]
        public bool IgnoreSpectating { get; set; }

        [XmlComment(Value = "In case we are not ignoring, minimum interval when checking for changes")]
        public int Interval { get; set; }

        [XmlComment(Value = "Set the value in the module using reflection")]
        public bool SetValueInModule { get; set; }

        [XmlComment(Value = "Call the Load function of the module after applying a change")]
        public bool CallLoad { get; set; }

        [XmlComment(Value = "Call the OnAwake function of the module after applying a change")]
        public bool CallOnAwake { get; set; }

        [XmlComment(Value = "Call the OnLoad function of the module after applying a change")]
        public bool CallOnLoad { get; set; }

        [XmlComment(Value = "Call the OnStart function of the module after applying a change")]
        public bool CallOnStart { get; set; }
    }
}
