using System;

namespace LmpCommon.Xml
{
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlCommentAttribute : Attribute
    {
        public string Value { get; set; }
    }
}
