using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Xml;

namespace Server.Utilities
{
    public static class ExtensionMethods
    {
        public static ConcurrentQueue<T> CloneConcurrentQueue<T>(this ConcurrentQueue<T> queue)
        {
            var messages = new T[queue.Count];
            queue.CopyTo(messages, 0);

            return new ConcurrentQueue<T>(messages);
        }

        public static string ToIndentedString(this XmlDocument doc)
        {
            using (var stringWriter = new StringWriter(new StringBuilder()))
            using (var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented })
            {
                doc.Save(xmlTextWriter);
                return stringWriter.ToString();
            }
        }
    }
}
