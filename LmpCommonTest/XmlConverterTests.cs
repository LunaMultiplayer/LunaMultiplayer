using LmpCommon.Xml;
using LmpCommonTest.Extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LmpCommonTest
{
    [TestClass]
    public class XmlConverterTests
    {
        private static readonly string XmlExamplePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "XmlExampleFiles");

        [TestMethod]
        public void TestConvertEvaToXml()
        {
            SwitchToXmlAndBack(Path.Combine(XmlExamplePath, "EVA.txt"));
        }

        [TestMethod]
        public void TestConvertVesselToXml()
        {
            SwitchToXmlAndBack(Path.Combine(XmlExamplePath, "Vessel.txt"));
        }

        [TestMethod]
        public void TestConvertBigVesselToXml()
        {
            SwitchToXmlAndBack(Path.Combine(XmlExamplePath, "BigVessel.txt"));
        }

        [TestMethod]
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

            var configNode = File.ReadAllText(filePath).ToUnixString();
            var xml = ConfigNodeXmlParser.ConvertToXml(configNode);
            var backToConfigNode = ConfigNodeXmlParser.ConvertToConfigNode(xml).ToUnixString();

            Assert.IsTrue(configNode.Equals(backToConfigNode), $"Error serializing config node. File: {Path.GetFileName(filePath)}");
        }
    }
}
