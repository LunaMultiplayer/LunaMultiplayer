using LunaCommon.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LMP.Tests
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
        
        private void SwitchToXmlAndBack(string filePath)
        {
            var configNode = File.ReadAllText(filePath);
            var xml = ConfigNodeXmlParser.ConvertToXml(configNode);
            var backToConfigNode = ConfigNodeXmlParser.ConvertToConfigNode(xml);

            Assert.AreEqual(configNode, backToConfigNode);
        }
    }
}
