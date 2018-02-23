using LunaCommon.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LMP.Tests
{
    [TestClass]
    public class XmlConverter
    {
        private static readonly string XmlExamplePath = Directory.GetCurrentDirectory() + "\\..\\..\\XmlExampleFiles\\";

        [TestMethod, Ignore]
        public void TestConvertEvaToXml()
        {
            SwitchToXmlAndBack("EVA.txt");
        }

        [TestMethod, Ignore]
        public void TestConvertVesselToXml()
        {
            SwitchToXmlAndBack("Vessel.txt");
        }

        [TestMethod, Ignore]
        public void TestConvertBigVesselToXml()
        {
            SwitchToXmlAndBack("BigVessel.txt");
        }

        private void SwitchToXmlAndBack(string fileName)
        {
            var configNode = File.ReadAllText(XmlExamplePath + fileName);
            var xml = ConfigNodeXmlParser.ConvertToXml(configNode);
            var backToConfigNode = ConfigNodeXmlParser.ConvertToConfigNode(xml);

            Assert.AreEqual(configNode, backToConfigNode);
        }
    }
}
