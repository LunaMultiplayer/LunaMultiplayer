using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace LunaCommon.Xml
{
    public class LunaXmlSerializer
    {
        public static void WriteXml(object objectToSerialize, string path)
        {
            try
            {
                using (var w = new XmlTextWriter(path, null))
                {
                    w.Formatting = Formatting.Indented;
                    var serializer = new XmlSerializer(objectToSerialize.GetType());
                    serializer.Serialize(w, objectToSerialize);
                }

                WriteComments(objectToSerialize, path);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not save xml to path {path}. Details: {e}");
            }
        }

        public static T ReadXml<T>(string path) where T:class, new()
        {
            if (!File.Exists(path))
                return null;
            try
            {
                using (TextReader r = new StreamReader(path))
                {
                    var deserializer = new XmlSerializer(typeof(T));
                    var structure = (T)deserializer.Deserialize(r);
                    return structure;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Could not open and read file from path {path}. Details: {e}");
            }
        }

        public static object ReadXml(Type classType, string path)
        {
            if (!File.Exists(path))
                return null;
            try
            {
                using (TextReader r = new StreamReader(path))
                {
                    var deserializer = new XmlSerializer(classType);
                    var structure = deserializer.Deserialize(r);
                    return structure;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Could not open and read file from path {path}. Details: {e}");
            }
        }

        private static void WriteComments(object objectToSerialize, string path)
        {
            try
            {
                var propertyComments = GetPropertiesAndComments(objectToSerialize);
                if (!propertyComments.Any()) return;

                var doc = new XmlDocument();
                doc.Load(path);

                var parent = doc.SelectSingleNode(objectToSerialize.GetType().Name);
                if (parent == null) return;

                var childNodes = parent.ChildNodes.Cast<XmlNode>().Where(n => propertyComments.ContainsKey(n.Name));
                foreach (var child in childNodes)
                {
                    parent.InsertBefore(doc.CreateComment(propertyComments[child.Name]), child);
                }

                doc.Save(path);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static Dictionary<string, string> GetPropertiesAndComments(object objectToSerialize)
        {
            var propertyComments = objectToSerialize.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(XmlCommentAttribute), false).Any())
                .Select(v => new
                {
                    v.Name,
                    ((XmlCommentAttribute) v.GetCustomAttributes(typeof(XmlCommentAttribute), false)[0]).Value
                })
                .ToDictionary(t => t.Name, t => t.Value);
            return propertyComments;
        }
    }
}
