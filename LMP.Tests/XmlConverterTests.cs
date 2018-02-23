using LunaCommon.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LMP.Tests
{
    [TestClass]
    public class XmlConverterTests
    {
        private static readonly string XmlExamplePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "XmlExampleFiles");

        [TestMethod, Ignore]
        public void TestConvertEvaToXml()
        {
            SwitchToXmlAndBack(Path.Combine(XmlExamplePath, "EVA.txt"));
        }

        [TestMethod, Ignore]
        public void TestConvertVesselToXml()
        {
            SwitchToXmlAndBack(Path.Combine(XmlExamplePath, "Vessel.txt"));
        }

        [TestMethod, Ignore]
        public void TestConvertBigVesselToXml()
        {
            SwitchToXmlAndBack(Path.Combine(XmlExamplePath, "BigVessel.txt"));
        }

        [TestMethod, Ignore]
        public void TestSeveralVesselsToXml()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(XmlExamplePath, "Others")))
            {
                SwitchToXmlAndBack(file);
            }
        }
        
        private static void SwitchToXmlAndBack(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var configNode = File.ReadAllText(filePath);
            var xml = ConfigNodeXmlParser.ConvertToXml(configNode);
            var backToConfigNode = ConfigNodeXmlParser.ConvertToConfigNode(xml);

            Assert.IsTrue(configNode.Equals(backToConfigNode), $"Error serializing config node. File: {Path.GetFileName(filePath)}");
        }
    }
}
