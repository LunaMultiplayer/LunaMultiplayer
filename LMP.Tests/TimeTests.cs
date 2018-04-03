using System;
using LunaCommon.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LMP.Tests
{
    [TestClass]
    public class TimeTests
    {
        [TestMethod]
        public void TestGetTime()
        {
            var date = TimeRetrieverNtp.GetNtpTime("time.google.com");
            Assert.AreNotEqual(DateTime.Now, date);
        }
    }
}
