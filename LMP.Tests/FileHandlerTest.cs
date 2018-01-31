using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.System;
using System.IO;
using System.Threading;

namespace LMP.Tests
{
    [TestClass]
    public class FileHandlerTest
    {
        [TestMethod]
        public void TestConsecutiveWritingToFile()
        {
            var filePath = Path.GetTempPath() + "TestFile.tmp";

            var task1 = new Thread(() => FileHandler.WriteToFile(filePath, "TASK1"));
            var task2 = new Thread(() => FileHandler.WriteToFile(filePath, "TASK2"));

            task1.Start();
            task2.Start();
            
            Thread.Sleep(100);

            Assert.AreEqual("TASK2", File.ReadAllText(filePath));

            File.Delete(filePath);
        }
    }
}
