using LmpCommon.CraftLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace LmpCommonTest
{
    [TestClass]
    public class CraftFolderQuarantineTests
    {
        private const string ValidCraft = "ship = Test\nversion = 1.12.5\nPART\n{\n\tpart = mk1pod\n}\n";
        private const string LoadMeta = "ship = Test\nversion = 1.12.5\npartNames = mk1pod\npartModules = ModuleCommand\n";

        private string _folder;

        [TestInitialize]
        public void CreateFolder()
        {
            _folder = Path.Combine(Path.GetTempPath(), "LmpQuarantineTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_folder);
        }

        [TestCleanup]
        public void DeleteFolder()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        private string Write(string relativePath, string content)
        {
            var path = Path.Combine(_folder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
            return path;
        }

        [TestMethod]
        public void PartlessCraftIsRenamedToInvalid()
        {
            var craft = Write("bad.craft", LoadMeta);

            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(_folder, SearchOption.TopDirectoryOnly);

            Assert.AreEqual(1, report.Moved.Count);
            Assert.AreEqual(0, report.Failures.Count);
            Assert.AreEqual(craft, report.Moved[0].Original);
            Assert.AreEqual(craft + ".invalid", report.Moved[0].Quarantined);
            Assert.IsFalse(File.Exists(craft));
            Assert.AreEqual(LoadMeta, File.ReadAllText(craft + ".invalid"));
        }

        [TestMethod]
        public void ValidCraftIsLeftAlone()
        {
            var craft = Write("good.craft", ValidCraft);

            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(_folder, SearchOption.TopDirectoryOnly);

            Assert.AreEqual(0, report.Moved.Count);
            Assert.AreEqual(0, report.Failures.Count);
            Assert.AreEqual(ValidCraft, File.ReadAllText(craft));
        }

        [TestMethod]
        public void NonCraftFilesAreLeftAlone()
        {
            var meta = Write("ship.loadmeta", LoadMeta);
            var text = Write("notes.txt", "no parts here");

            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(_folder, SearchOption.TopDirectoryOnly);

            Assert.AreEqual(0, report.Moved.Count);
            Assert.IsTrue(File.Exists(meta));
            Assert.IsTrue(File.Exists(text));
        }

        [TestMethod]
        public void ExistingQuarantinedCopyIsReplacedWithoutNumberedSiblings()
        {
            var craft = Write("bad.craft", LoadMeta + "second\n");
            Write("bad.craft.invalid", "old quarantined copy");

            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(_folder, SearchOption.TopDirectoryOnly);

            Assert.AreEqual(1, report.Moved.Count);
            Assert.AreEqual(0, report.Failures.Count);
            Assert.IsFalse(File.Exists(craft));
            Assert.AreEqual(LoadMeta + "second\n", File.ReadAllText(craft + ".invalid"));
            CollectionAssert.AreEqual(new[] { "bad.craft.invalid" }, Directory.GetFiles(_folder).Select(Path.GetFileName).ToArray());
        }

        [TestMethod]
        public void RepeatedRestoresKeepASingleQuarantinedFile()
        {
            for (var i = 0; i < 3; i++)
            {
                Write("bad.craft", LoadMeta + i + "\n");
                CraftFolderQuarantine.QuarantinePartlessCrafts(_folder, SearchOption.TopDirectoryOnly);
            }

            CollectionAssert.AreEqual(new[] { "bad.craft.invalid" }, Directory.GetFiles(_folder).Select(Path.GetFileName).ToArray());
            Assert.AreEqual(LoadMeta + "2\n", File.ReadAllText(Path.Combine(_folder, "bad.craft.invalid")));
        }

        [TestMethod]
        public void TopDirectoryOnlyIgnoresSubfolders()
        {
            var nested = Write(Path.Combine("player", "VAB", "bad.craft"), LoadMeta);

            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(_folder, SearchOption.TopDirectoryOnly);

            Assert.AreEqual(0, report.Moved.Count);
            Assert.IsTrue(File.Exists(nested));
        }

        [TestMethod]
        public void AllDirectoriesFindsNestedCrafts()
        {
            var nested = Write(Path.Combine("player", "VAB", "bad.craft"), LoadMeta);
            var good = Write(Path.Combine("player", "SPH", "good.craft"), ValidCraft);

            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(_folder, SearchOption.AllDirectories);

            Assert.AreEqual(1, report.Moved.Count);
            Assert.IsFalse(File.Exists(nested));
            Assert.IsTrue(File.Exists(nested + ".invalid"));
            Assert.IsTrue(File.Exists(good));
        }

        [TestMethod]
        public void MissingFolderIsAnEmptyReport()
        {
            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(Path.Combine(_folder, "missing"), SearchOption.AllDirectories);

            Assert.AreEqual(0, report.Moved.Count);
            Assert.AreEqual(0, report.Failures.Count);
        }

        [TestMethod]
        public void FailedMoveIsReportedAndLeavesTheCraftInPlace()
        {
            var craft = Write("bad.craft", LoadMeta);
            Directory.CreateDirectory(craft + ".invalid");

            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(_folder, SearchOption.TopDirectoryOnly);

            Assert.AreEqual(0, report.Moved.Count);
            Assert.AreEqual(1, report.Failures.Count);
            Assert.AreEqual(craft, report.Failures[0].Path);
            StringAssert.Contains(report.Failures[0].Message, "Could not quarantine craft");
            Assert.AreEqual(LoadMeta, File.ReadAllText(craft));
        }

        [TestMethod]
        public void OneFailureDoesNotStopTheRestOfThePass()
        {
            var blocked = Write("a.craft", LoadMeta);
            Directory.CreateDirectory(blocked + ".invalid");
            var other = Write("b.craft", LoadMeta);

            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(_folder, SearchOption.TopDirectoryOnly);

            Assert.AreEqual(1, report.Failures.Count);
            Assert.AreEqual(1, report.Moved.Count);
            Assert.IsTrue(File.Exists(other + ".invalid"));
        }
    }
}
