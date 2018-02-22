using LunaCommon.Xml;

namespace LunaClient.ModuleStore.Structures
{
    public class CustomFieldDefinition
    {
        [XmlComment(Value = "Name of the field that we are customizing")]
        public string FieldName { get; set; }

        [XmlComment(Value = "Ignore changes on this field when sending data")]
        public bool IgnoreSend { get; set; }

        [XmlComment(Value = "Ignore changes on this field when receiveing data")]
        public bool IgnoreReceive { get; set; }

        [XmlComment(Value = "In case we are not ignoring, minimum interval when checking for changes")]
        public int IntervalCheckChangesMs { get; set; }

        [XmlComment(Value = "In case we are not ignoring, minimum interval when aplying the changes we received")]
        public int IntervalApplyChangesMs { get; set; }
    }
}
