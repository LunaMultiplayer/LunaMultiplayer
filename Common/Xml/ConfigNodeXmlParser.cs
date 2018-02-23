using System.IO;
using System.Xml;

namespace LunaCommon.Xml
{
    /// <summary>
    /// I really hate the config node structure so this class converts a config node to a XML and a XML to a config node
    /// </summary>
    public static class ConfigNodeXmlParser
    {
        private const string StartElement = "LMPConfigNodeToXML";
        private const string ValueNode = "VALUE";
        private const string AttributeName = "name";

        public static string ConvertToXml(string configNode)
        {
            using (var reader = new StringReader(configNode))
            using (var writer = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings() { Indent = true, CheckCharacters = false }))
            {
                xmlWriter.WriteStartElement(StartElement);
                var previous = string.Empty;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(" = "))
                    {
                        WriteXmlValue(line, xmlWriter);
                    }
                    else if (line.Contains("{") && line.Trim().Length == 1)
                    {
                        xmlWriter.WriteStartElement(previous.Trim());
                    }
                    else if (line.Contains("}") && line.Trim().Length == 1)
                    {
                        xmlWriter.WriteEndElement();
                    }
                    else
                    {
                        previous = line;
                    }
                }
                xmlWriter.WriteEndElement();
                xmlWriter.Close();
                return writer.ToString();
            }
        }

        public static string ConvertToConfigNode(string xmlDoc)
        {
            using (var reader = new StringReader(xmlDoc))
            using (var xmlReader = new XmlTextReader(reader))
            using (var writer = new StringWriter())
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xmlReader.Name != StartElement)
                            {
                                if (xmlReader.Name == ValueNode)
                                {
                                    writer.Write(GetDepthTabs(xmlReader.Depth - 1) + xmlReader.GetAttribute(AttributeName) + " = ");
                                }
                                else
                                {
                                    writer.WriteLine(GetDepthTabs(xmlReader.Depth - 1) + xmlReader.Name);
                                    writer.WriteLine(GetDepthTabs(xmlReader.Depth - 1) + "{");
                                    if(xmlReader.IsEmptyElement)
                                        writer.WriteLine(GetDepthTabs(xmlReader.Depth - 1) + "}");
                                }
                            }
                            break;
                        case XmlNodeType.Text:
                            writer.WriteLine(xmlReader.Value);
                            break;
                        case XmlNodeType.EndElement:
                            if (xmlReader.Name != StartElement && xmlReader.Name != ValueNode)
                            {
                                writer.WriteLine(GetDepthTabs(xmlReader.Depth - 1) + "}");
                            }
                            break;
                    }
                }
                
                return writer.ToString().Trim();
            }
        }

        private static string GetDepthTabs(int depth)
        {
            var tabs = string.Empty;

            for (var i = 0; i < depth; i++)
            {
                tabs = tabs + "\t";
            }

            return tabs;
        }

        private static void WriteXmlValue(string line, XmlWriter xmlWriter)
        {
            var keyVal = line.Split('=');
            if (keyVal.Length == 2)
            {
                xmlWriter.WriteStartElement(ValueNode);
                xmlWriter.WriteAttributeString(AttributeName, keyVal[0].Trim());
                xmlWriter.WriteValue(keyVal[1].Trim());
                xmlWriter.WriteEndElement();
            }
            else if (keyVal.Length == 1)
            {
                xmlWriter.WriteStartElement(ValueNode);
                xmlWriter.WriteAttributeString(AttributeName, keyVal[0].Trim());
                xmlWriter.WriteEndElement();
            }
        }
    }
}
