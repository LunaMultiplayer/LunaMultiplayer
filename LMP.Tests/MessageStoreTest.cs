using System;
using LunaClient.Systems.VesselProtoSys;
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
        private static readonly Random Rnd = new Random();
        
        [TestMethod]
        public void TestMsgMessageStore()
        {
            var msg1 = Factory.CreateNew<VesselSrvMsg, VesselPositionMsgData>();
            
            Assert.AreEqual(0, MessageStore.GetMessageCount(typeof(VesselSrvMsg)));
            Assert.AreEqual(0, MessageStore.GetMessageDataCount(typeof(VesselPositionMsgData)));

            var msg2 = Factory.CreateNew<VesselSrvMsg, VesselPositionMsgData>();
            
            Assert.AreEqual(0, MessageStore.GetMessageCount(typeof(VesselSrvMsg)));
            Assert.AreEqual(0, MessageStore.GetMessageDataCount(typeof(VesselPositionMsgData)));

            //Set first message as "used"
            msg1.Recycle();

            Assert.AreEqual(1, MessageStore.GetMessageCount(typeof(VesselSrvMsg)));
            Assert.AreEqual(1, MessageStore.GetMessageDataCount(typeof(VesselPositionMsgData)));
            
            //If we retrieve a new message the first one should be reused
            var msg3 = Factory.CreateNew<VesselSrvMsg, VesselPositionMsgData>();
            
            msg2.Recycle();
            msg3.Recycle();

            Assert.AreEqual(2, MessageStore.GetMessageCount(typeof(VesselSrvMsg)));
            Assert.AreEqual(2, MessageStore.GetMessageDataCount(typeof(VesselPositionMsgData)));

            var msg4 = Factory.CreateNew<VesselSrvMsg, VesselPositionMsgData>();
            
            Assert.AreEqual(1, MessageStore.GetMessageCount(typeof(VesselSrvMsg)));
            Assert.AreEqual(1, MessageStore.GetMessageDataCount(typeof(VesselPositionMsgData)));
        }

        [TestMethod]
        public void TestMsgSerializeReuseDeserialize()
        {
            var msg1 = Factory.CreateNew<VesselSrvMsg, VesselProtoMsgData>();
            var msgData1 = (VesselProtoMsgData)msg1.Data;

            msgData1.VesselData = new byte[100];
            Rnd.NextBytes(msgData1.VesselData);

            var serializedMsg1 = msg1.Serialize(true);

            var msg2 = Factory.CreateNew<VesselSrvMsg, VesselRemoveMsgData>();
            var msgData2 = (VesselRemoveMsgData)msg1.Data;

            msgData2.VesselId = Guid.NewGuid();

            var serializedMsg2 = msg2.Serialize(true);

            msg1 = Factory.Deserialize(serializedMsg1, DateTime.Now.Ticks) as VesselSrvMsg;
            msg2 = Factory.Deserialize(serializedMsg2, DateTime.Now.Ticks) as VesselSrvMsg;
        }
    }
}
