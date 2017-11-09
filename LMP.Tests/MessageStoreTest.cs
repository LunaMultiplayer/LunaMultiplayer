using LunaCommon.Message;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LMP.Tests
{
    [TestClass]
    public class MessageStoreTest
    {
        private static readonly ServerMessageFactory Factory = new ServerMessageFactory();
        
        [TestMethod]
        public void TestMsgMessageStore()
        {
            var msg1 = Factory.CreateNew<VesselSrvMsg, VesselPositionMsgData>();
            Assert.AreEqual(1, MessageStore.GetMessageCount(typeof(VesselSrvMsg)));
            Assert.AreEqual(1, MessageStore.GetMessageDataCount(typeof(VesselPositionMsgData)));
            Assert.IsFalse(msg1.Data.ReadyToRecycle);

            var msg2 = Factory.CreateNew<VesselSrvMsg, VesselPositionMsgData>();
            Assert.AreEqual(2, MessageStore.GetMessageCount(typeof(VesselSrvMsg)));
            Assert.AreEqual(2, MessageStore.GetMessageDataCount(typeof(VesselPositionMsgData)));
            Assert.IsFalse(msg2.Data.ReadyToRecycle);

            //Set first message as "used"
            msg1.Data.ReadyToRecycle = true;

            //If we retrieve a new message the first one should be reused
            var msg3 = Factory.CreateNew<VesselSrvMsg, VesselPositionMsgData>();
            Assert.AreEqual(2, MessageStore.GetMessageCount(typeof(VesselSrvMsg)));
            Assert.AreEqual(2, MessageStore.GetMessageDataCount(typeof(VesselPositionMsgData)));
            Assert.IsFalse(msg3.Data.ReadyToRecycle);

            msg2.Data.ReadyToRecycle = true;
            msg3.Data.ReadyToRecycle = true;

            var msg4 = Factory.CreateNew<VesselSrvMsg, VesselPositionMsgData>();
            Assert.AreEqual(2, MessageStore.GetMessageCount(typeof(VesselSrvMsg)));
            Assert.AreEqual(2, MessageStore.GetMessageDataCount(typeof(VesselPositionMsgData)));
            Assert.IsFalse(msg4.Data.ReadyToRecycle);
        }
    }
}
