using LunaConfigNode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Structures;
using System.IO;

namespace ServerTest
{
    [TestClass]
    public class VesselTest
    {
        private static readonly string XmlExamplePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "XmlExampleFiles", "Others");

        [TestMethod]
        public void TestCreateVessel()
        {
            foreach (var file in Directory.GetFiles(XmlExamplePath))
            {
                var cfgNode = new ConfigNode(File.ReadAllText(file));
                var vessel = new Vessel(cfgNode);
                Assert.IsNotNull(vessel);
            }
        }
    }
}
