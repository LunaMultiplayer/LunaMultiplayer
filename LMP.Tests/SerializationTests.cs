using LunaCommon.Message;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LMP.Tests
{
    [TestClass]
    public class SerializationTests
    {
        private static readonly ServerMessageFactory Factory = new ServerMessageFactory();

        [TestMethod]
        public void TestSerializeDeserialize()
        {
            var bytes = new byte[10000];
            new Random().NextBytes(bytes);

            var msgData = Factory.CreateNewMessageData<VesselProtoMsgData>();
            msgData.VesselId = Guid.NewGuid();
            msgData.VesselData = bytes;
            msgData.SentTime = DateTime.UtcNow.Ticks;

            var msg = Factory.CreateNew<VesselSrvMsg>(msgData);

            //Serialize and compress
            var serialized = msg.Serialize(true);
            //Serialize no compress
            var serializedNc = msg.Serialize(false);

            //Deserialize compressedMsg
            var msg2 = Factory.Deserialize(serialized, Environment.TickCount);
            //Deserialize no compressed message
            var msg2Nc = Factory.Deserialize(serializedNc, Environment.TickCount);

           Assert.IsTrue(((VesselProtoMsgData)msg.Data).VesselData.SequenceEqual(((VesselProtoMsgData)msg2.Data).VesselData) &&
                     ((VesselProtoMsgData)msg2Nc.Data).VesselData.SequenceEqual(((VesselProtoMsgData)msg2.Data).VesselData));
        }
    }
}
