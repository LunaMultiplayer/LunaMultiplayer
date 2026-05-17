using LmpCommon.Locks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.System;
using System;
using System.Linq;

namespace ServerTest
{
    [TestClass]
    public class LockSystemTest
    {
        private Guid _vessel1 = Guid.NewGuid();
        private Guid _vessel2 = Guid.NewGuid();
        private string _player1 = "Player1";
        private string _player2 = "Player2";

        [TestInitialize]
        public void Setup()
        {
            var allLocks = LockSystem.LockQuery.GetAllLocks().ToList();
            foreach (var l in allLocks)
            {
                LockSystem.ReleaseLock(l);
            }
        }

        [TestMethod]
        public void TestAcquireAndReleaseLock()
        {
            var lockDef = new LockDefinition(LockType.Control, _player1, _vessel1);

            // Acquire
            Assert.IsTrue(LockSystem.AcquireLock(lockDef, false, out var repeated));
            Assert.IsFalse(repeated);
            Assert.IsTrue(LockSystem.LockQuery.LockBelongsToPlayer(LockType.Control, _vessel1, null, _player1));

            // Release
            Assert.IsTrue(LockSystem.ReleaseLock(lockDef));
            Assert.IsFalse(LockSystem.LockQuery.LockExists(LockType.Control, _vessel1, null));
        }

        [TestMethod]
        public void TestCannotAcquireAlreadyOwnedLock()
        {
            var lockDef1 = new LockDefinition(LockType.Control, _player1, _vessel1);
            var lockDef2 = new LockDefinition(LockType.Control, _player2, _vessel1);

            Assert.IsTrue(LockSystem.AcquireLock(lockDef1, false, out var repeated));

            // Player2 tries to take it
            Assert.IsFalse(LockSystem.AcquireLock(lockDef2, false, out repeated));
            Assert.IsTrue(LockSystem.LockQuery.LockBelongsToPlayer(LockType.Control, _vessel1, null, _player1));
        }

        [TestMethod]
        public void TestForceAcquireLock()
        {
            var lockDef1 = new LockDefinition(LockType.Control, _player1, _vessel1);
            var lockDef2 = new LockDefinition(LockType.Control, _player2, _vessel1);

            LockSystem.AcquireLock(lockDef1, false, out var repeated);

            // Player2 forces it
            Assert.IsTrue(LockSystem.AcquireLock(lockDef2, true, out repeated));
            Assert.IsTrue(LockSystem.LockQuery.LockBelongsToPlayer(LockType.Control, _vessel1, null, _player2));
        }

        [TestMethod]
        public void TestOnlyOneControlLockPerPlayer()
        {
            var lockDef1 = new LockDefinition(LockType.Control, _player1, _vessel1);
            var lockDef2 = new LockDefinition(LockType.Control, _player1, _vessel2);

            LockSystem.AcquireLock(lockDef1, false, out var repeated);
            Assert.IsTrue(LockSystem.LockQuery.LockExists(LockType.Control, _vessel1, null));

            // Acquire second control lock, first should be released
            LockSystem.AcquireLock(lockDef2, false, out repeated);

            Assert.IsFalse(LockSystem.LockQuery.LockExists(LockType.Control, _vessel1, null));
            Assert.IsTrue(LockSystem.LockQuery.LockExists(LockType.Control, _vessel2, null));
        }
    }
}
