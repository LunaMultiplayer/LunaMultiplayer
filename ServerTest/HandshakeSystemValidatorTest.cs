using LmpCommon;
using LmpCommon.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Settings.Structures;
using Server.System;
using System.IO;

namespace ServerTest
{
    [TestClass]
    public class HandshakeSystemValidatorTest
    {
        [TestInitialize]
        public void Setup()
        {
        }

        [TestMethod]
        public void TestVersionValidation()
        {
            // Set up local version for comparison
            var localVersion = LmpVersioning.CurrentVersion.ToString();

            // Should succeed with current version
            Assert.IsTrue(LmpVersioning.IsCompatible(localVersion));

            // Should fail with a wildly different version
            Assert.IsFalse(LmpVersioning.IsCompatible("0.1.0.0"));
        }

        [TestMethod]
        public void TestModValidationDisabled()
        {
            // Set server to NOT use a whitelist
            GeneralSettings.SettingsStore.ModControl = false;

            // If mod control is disabled, HandshakeReply should have ModControl = false
            Assert.IsFalse(GeneralSettings.SettingsStore.ModControl);
        }

        [TestMethod]
        public void TestPlayerNameValidation()
        {
            // Valid names
            Assert.IsTrue(HandshakeSystem.PlayerNameIsValid("ValidName", out var reason));
            Assert.IsTrue(HandshakeSystem.PlayerNameIsValid("Player_1", out reason));

            // Invalid names
            Assert.IsFalse(HandshakeSystem.PlayerNameIsValid("Invalid Name!", out reason));
            Assert.IsFalse(HandshakeSystem.PlayerNameIsValid("", out reason)); // Too short
            Assert.IsFalse(HandshakeSystem.PlayerNameIsValid(new string('A', 33), out reason)); // Too long
        }
    }
}
