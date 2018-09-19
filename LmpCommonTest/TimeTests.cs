using LmpCommon.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LmpCommonTest
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
