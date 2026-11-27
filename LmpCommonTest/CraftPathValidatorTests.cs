using LmpCommon.CraftLibrary;
using LmpCommon.Enums;
using LmpCommon.Message.Data.CraftLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LmpCommonTest
{
    [TestClass]
    public class CraftPathValidatorTests
    {
        [DataTestMethod]
        [DataRow("Hengas's Planetary Passby")]
        [DataRow("[Kerbin] Space Station")]
        [DataRow("Duna Mining Station, Part 1")]
        [DataRow("Mun Land + Return")]
        [DataRow("Relay 1_12Gm")]
        [DataRow("Ünïcode ship")]
        [DataRow("Console")]
        [DataRow("COM10")]
        [DataRow("v1.2 rocket")]
        [DataRow("Going to Duna...")]
        [DataRow("Mun Lander.")]
        [DataRow("Ship ")]
        [DataRow(".")]
        [DataRow("..")]
        public void ValidCraftNamesAreAccepted(string name)
        {
            Assert.IsTrue(CraftPathValidator.IsSafeCraftName(name));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("../other")]
        [DataRow("..\\other")]
        [DataRow("a/b")]
        [DataRow("a\\b")]
        [DataRow("C:\\temp")]
        [DataRow("name:stream")]
        [DataRow("what?")]
        [DataRow("star*")]
        [DataRow("quote\"d")]
        [DataRow("<tag>")]
        [DataRow("pipe|name")]
        [DataRow("line\nbreak")]
        [DataRow("nul\0char")]
        [DataRow("CON")]
        [DataRow("con")]
        [DataRow("NUL.txt")]
        [DataRow("aux.craft")]
        [DataRow("com1")]
        [DataRow("LPT9")]
        public void UnsafeCraftNamesAreRejected(string name)
        {
            Assert.IsFalse(CraftPathValidator.IsSafeCraftName(name));
        }

        [TestMethod]
        public void CraftNameLengthIsBounded()
        {
            Assert.IsTrue(CraftPathValidator.IsSafeCraftName(new string('a', 200)));
            Assert.IsFalse(CraftPathValidator.IsSafeCraftName(new string('a', 201)));
        }

        [DataTestMethod]
        [DataRow("Drew")]
        [DataRow("Player-1_a")]
        [DataRow("Some Folder")]
        [DataRow("v1.2")]
        [DataRow("Console")]
        public void ValidFolderNamesAreAccepted(string name)
        {
            Assert.IsTrue(CraftPathValidator.IsSafeFolderName(name));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow(".")]
        [DataRow("..")]
        [DataRow("trailing.")]
        [DataRow("trailing ")]
        [DataRow("../other")]
        [DataRow("a/b")]
        [DataRow("a\\b")]
        [DataRow("C:")]
        [DataRow("name:stream")]
        [DataRow("what?")]
        [DataRow("line\nbreak")]
        [DataRow("CON")]
        [DataRow("nul")]
        public void UnsafeFolderNamesAreRejected(string name)
        {
            Assert.IsFalse(CraftPathValidator.IsSafeFolderName(name));
        }

        [TestMethod]
        public void FolderNameLengthIsBounded()
        {
            Assert.IsTrue(CraftPathValidator.IsSafeFolderName(new string('a', 128)));
            Assert.IsFalse(CraftPathValidator.IsSafeFolderName(new string('a', 129)));
        }

        [TestMethod]
        public void KnownTypesAreAccepted()
        {
            Assert.IsTrue(CraftPathValidator.IsKnownType(CraftType.Vab));
            Assert.IsTrue(CraftPathValidator.IsKnownType(CraftType.Sph));
            Assert.IsTrue(CraftPathValidator.IsKnownType(CraftType.Subassembly));
        }

        [TestMethod]
        public void UndefinedTypesAreRejected()
        {
            Assert.IsFalse(CraftPathValidator.IsKnownType((CraftType)3));
            Assert.IsFalse(CraftPathValidator.IsKnownType((CraftType)(-1)));
            Assert.IsFalse(CraftPathValidator.IsKnownType((CraftType)255));
        }

        [TestMethod]
        public void CraftInfoNeedsFolderNameAndTypeToAllBeSafe()
        {
            Assert.IsTrue(CraftPathValidator.IsSafe(new CraftInfo { FolderName = "Drew", CraftName = "Ship", CraftType = CraftType.Vab }));
            Assert.IsTrue(CraftPathValidator.IsSafe(new CraftInfo { FolderName = "Drew", CraftName = "Going to Duna...", CraftType = CraftType.Vab }));
            Assert.IsFalse(CraftPathValidator.IsSafe(new CraftInfo { FolderName = "..", CraftName = "Ship", CraftType = CraftType.Vab }));
            Assert.IsFalse(CraftPathValidator.IsSafe(new CraftInfo { FolderName = "Drew", CraftName = "..\\Ship", CraftType = CraftType.Vab }));
            Assert.IsFalse(CraftPathValidator.IsSafe(new CraftInfo { FolderName = "Drew", CraftName = "Ship", CraftType = (CraftType)9 }));
            Assert.IsFalse(CraftPathValidator.IsSafe((CraftInfo)null));
        }

        [TestMethod]
        public void CraftBasicInfoNeedsFolderNameAndTypeToAllBeSafe()
        {
            Assert.IsTrue(CraftPathValidator.IsSafe(new CraftBasicInfo { FolderName = "Drew", CraftName = "Ship", CraftType = CraftType.Sph }));
            Assert.IsTrue(CraftPathValidator.IsSafe(new CraftBasicInfo { FolderName = "Drew", CraftName = "Mun Lander.", CraftType = CraftType.Sph }));
            Assert.IsFalse(CraftPathValidator.IsSafe(new CraftBasicInfo { FolderName = "a/b", CraftName = "Ship", CraftType = CraftType.Sph }));
            Assert.IsFalse(CraftPathValidator.IsSafe(new CraftBasicInfo { FolderName = "Drew.", CraftName = "Ship", CraftType = CraftType.Sph }));
            Assert.IsFalse(CraftPathValidator.IsSafe(new CraftBasicInfo { FolderName = "Drew", CraftName = "", CraftType = CraftType.Sph }));
            Assert.IsFalse(CraftPathValidator.IsSafe(new CraftBasicInfo { FolderName = "Drew", CraftName = "Ship", CraftType = (CraftType)3 }));
            Assert.IsFalse(CraftPathValidator.IsSafe((CraftBasicInfo)null));
        }
    }
}
