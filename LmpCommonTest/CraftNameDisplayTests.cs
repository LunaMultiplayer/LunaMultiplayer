using LmpCommon.CraftLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LmpCommonTest
{
    [TestClass]
    public class CraftNameDisplayTests
    {
        [DataTestMethod]
        [DataRow("Hengas's Planetary Passby")]
        [DataRow("Going to Duna...")]
        [DataRow("Ünïcode ship")]
        [DataRow("日本語の船")]
        public void OrdinaryNamesAreUnchanged(string name)
        {
            Assert.AreEqual(name, CraftNameDisplay.Sanitize(name));
        }

        [TestMethod]
        public void RichTextTagsLoseTheirBrackets()
        {
            Assert.AreEqual("size=500Huge", CraftNameDisplay.Sanitize("<size=500>Huge"));
            Assert.AreEqual("color=redText/color", CraftNameDisplay.Sanitize("<color=red>Text</color>"));
        }

        [TestMethod]
        public void ControlCharactersAreRemoved()
        {
            Assert.AreEqual("ab", CraftNameDisplay.Sanitize("a\r\nb"));
            Assert.AreEqual("ab", CraftNameDisplay.Sanitize("a\0b"));
            Assert.AreEqual("ab", CraftNameDisplay.Sanitize("a\tb"));
        }

        [TestMethod]
        public void InvisibleReorderingCharactersAreRemoved()
        {
            Assert.AreEqual("gpj.exe", CraftNameDisplay.Sanitize("gp\u202Ej.exe"));
            Assert.AreEqual("ab", CraftNameDisplay.Sanitize("a\u200Bb"));
        }

        [TestMethod]
        public void NullEmptyAndFullyStrippedNamesGetAPlaceholder()
        {
            Assert.AreEqual("?", CraftNameDisplay.Sanitize(null));
            Assert.AreEqual("?", CraftNameDisplay.Sanitize(string.Empty));
            Assert.AreEqual("?", CraftNameDisplay.Sanitize("<>\r\n"));
        }

        [TestMethod]
        public void LongNamesAreShortenedWithAnEllipsis()
        {
            Assert.AreEqual(new string('a', 50), CraftNameDisplay.Sanitize(new string('a', 50)));
            Assert.AreEqual(new string('a', 50) + "...", CraftNameDisplay.Sanitize(new string('a', 51)));
        }

        [TestMethod]
        public void ShorteningNeverSplitsASurrogatePair()
        {
            var emoji = "\U0001F680";
            var name = new string('a', 49) + emoji + "tail";

            var result = CraftNameDisplay.Sanitize(name);

            Assert.AreEqual(new string('a', 49) + "...", result);
        }

        [TestMethod]
        public void LengthIsMeasuredAfterStripping()
        {
            var name = new string('a', 50) + "<><><>";

            Assert.AreEqual(new string('a', 50), CraftNameDisplay.Sanitize(name));
        }
    }
}
