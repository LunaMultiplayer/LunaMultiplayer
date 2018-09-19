using LmpCommon.Xml;

namespace LmpClient.ModuleStore.Structures
{
    public class MethodDefinition
    {
        [XmlComment(Value = "Name of the method that we are customizing")]
        public string MethodName { get; set; }

        [XmlComment(Value = "Max interval to sync this method call")]
        public int MaxIntervalInMs { get; set; }
    }
}
