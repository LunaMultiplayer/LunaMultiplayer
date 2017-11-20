using LunaCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LMP.Tests
{
    [TestClass]
    public class TimeRetrieverTests
    {
        [TestMethod]
        public void RetrieveNistTime()
        {
            var returnValue = TimeRetriever.GetNistTimeFromWeb();
            Assert.AreNotEqual(returnValue, DateTime.MinValue);
            
            returnValue = TimeRetriever.GetNistDateTimeFromSocket();
            Assert.AreNotEqual(returnValue, DateTime.MinValue);
        }
    }
}
