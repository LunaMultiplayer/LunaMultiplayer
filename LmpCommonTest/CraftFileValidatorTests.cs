using LmpCommon.CraftLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace LmpCommonTest
{
    [TestClass]
    public class CraftFileValidatorTests
    {
        private const string ValidCraft = "ship = Test\nversion = 1.12.5\nPART\n{\n\tname = mk1pod.v2\n}\n";

        private const string LoadMetaOnly = "shipName = Test\npartCount = 64\npartNames = mk1pod.v2\npartModules = ModuleCommand\n";

        [TestMethod]
        public void CraftWithPartBlockIsAccepted()
        {
            var data = Encoding.UTF8.GetBytes(ValidCraft);
            Assert.IsTrue(CraftFileValidator.HasParts(data, data.Length));
        }

        [TestMethod]
        public void LoadMetaSummaryIsRejected()
        {
            var data = Encoding.UTF8.GetBytes(LoadMetaOnly);
            Assert.IsFalse(CraftFileValidator.HasParts(data, data.Length));
        }

        [TestMethod]
        public void CrlfLineEndingsAreAccepted()
        {
            var data = Encoding.UTF8.GetBytes(ValidCraft.Replace("\n", "\r\n"));
            Assert.IsTrue(CraftFileValidator.HasParts(data, data.Length));
        }

        [TestMethod]
        public void PartTextInsideAnotherValueIsRejected()
        {
            var data = Encoding.UTF8.GetBytes("description = PART\npartNames = PART_thing\n");
            Assert.IsFalse(CraftFileValidator.HasParts(data, data.Length));
        }

        [TestMethod]
        public void OnlyBytesUpToNumBytesAreChecked()
        {
            var data = Encoding.UTF8.GetBytes(ValidCraft);
            var lengthBeforePart = Encoding.UTF8.GetByteCount("ship = Test\nversion = 1.12.5\n");
            Assert.IsFalse(CraftFileValidator.HasParts(data, lengthBeforePart));
        }

        [TestMethod]
        public void NullEmptyOrOversizedInputIsRejected()
        {
            var data = Encoding.UTF8.GetBytes(ValidCraft);
            Assert.IsFalse(CraftFileValidator.HasParts(null, 0));
            Assert.IsFalse(CraftFileValidator.HasParts(data, 0));
            Assert.IsFalse(CraftFileValidator.HasParts(data, data.Length + 1));
        }
    }
}
