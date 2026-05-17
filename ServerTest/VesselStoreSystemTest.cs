using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Context;
using Server.System;
using System;
using System.IO;
using System.Linq;

namespace ServerTest
{
    [TestClass]
    public class VesselStoreSystemTest
    {
        private Guid _vessel1 = Guid.NewGuid();
        private static readonly string XmlExamplePath = Path.Combine(Directory.GetCurrentDirectory(), "XmlExampleFiles", "Vessel.txt");

        [TestInitialize]
        public void Setup()
        {
            // Set up a mock universe directory
            ServerContext.UniverseDirectory = Path.Combine(Path.GetTempPath(), "LMPTestUniverse_" + Guid.NewGuid());
            if (!Directory.Exists(ServerContext.UniverseDirectory))
                Directory.CreateDirectory(ServerContext.UniverseDirectory);

            VesselStoreSystem.VesselsPath = Path.Combine(ServerContext.UniverseDirectory, "Vessels");
            if (!Directory.Exists(VesselStoreSystem.VesselsPath))
                Directory.CreateDirectory(VesselStoreSystem.VesselsPath);

            VesselStoreSystem.CurrentVessels.Clear();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(ServerContext.UniverseDirectory))
                Directory.Delete(ServerContext.UniverseDirectory, true);
        }

        private string GetValidVesselData(Guid id)
        {
            var content = File.ReadAllText(XmlExamplePath);
            return content.Replace("pid = 40660d72-4818-4df7-9e9f-7e038abafdea", "pid = " + id.ToString("D"));
        }

        [TestMethod]
        public void TestStoreAndRetrieveVessel()
        {
            var vesselData = GetValidVesselData(_vessel1);

            // Add to dictionary and backup to file
            VesselStoreSystem.CurrentVessels.TryAdd(_vessel1, new Server.System.Vessel.Classes.Vessel(vesselData));
            VesselStoreSystem.BackupVessels();

            // Check file exists
            var expectedPath = Path.Combine(VesselStoreSystem.VesselsPath, _vessel1 + ".txt");
            Assert.IsTrue(File.Exists(expectedPath), "Vessel file should be created");

            // Retrieve vessel
            var retrievedData = VesselStoreSystem.GetVesselInConfigNodeFormat(_vessel1);
            Assert.IsNotNull(retrievedData);
        }

        [TestMethod]
        public void TestRemoveVessel()
        {
            var vesselData = GetValidVesselData(_vessel1);
            VesselStoreSystem.CurrentVessels.TryAdd(_vessel1, new Server.System.Vessel.Classes.Vessel(vesselData));
            VesselStoreSystem.BackupVessels();

            VesselStoreSystem.RemoveVessel(_vessel1);

            // We need to wait a bit as RemoveVessel runs FileDelete in a Task.Run
            System.Threading.Thread.Sleep(200);

            var expectedPath = Path.Combine(VesselStoreSystem.VesselsPath, _vessel1 + ".txt");
            Assert.IsFalse(File.Exists(expectedPath), "Vessel file should be deleted");
            Assert.IsFalse(VesselStoreSystem.VesselExists(_vessel1), "Vessel should be removed from dictionary");
        }
    }
}
