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
            //The threads should not take longer than 60 seconds.  If either of them timeout after 60 seconds, fail.
            task1.Join(60000);
            task2.Join(60000);

            Assert.AreEqual("TASK2", File.ReadAllText(filePath));

            File.Delete(filePath);
        }
    }
}
