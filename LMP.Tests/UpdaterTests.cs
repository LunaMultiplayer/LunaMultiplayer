using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Updater;

namespace LMP.Tests
{
    [TestClass]
    public class UpdaterTests
    {
        [TestMethod]
        public void TestGetLatestVersion()
        {
            var latestVersion = UpdateChecker.GetLatestVersion();
        }
    }
}
