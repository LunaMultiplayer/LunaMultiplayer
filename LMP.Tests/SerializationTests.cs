using Lidgren.Network;
using LunaCommon.Message;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using LunaCommon.Time;
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
            var client = new NetClient(new NetPeerConfiguration("TESTS"));
            var bytes = new byte[10000];
            new Random().NextBytes(bytes);

            var msgData = Factory.CreateNewMessageData<VesselProtoMsgData>();
            msgData.Vessel.VesselId = Guid.NewGuid();
            msgData.Vessel.Data = bytes;
            msgData.Vessel.NumBytes = bytes.Length;
            msgData.SentTime = LunaTime.UtcNow.Ticks;

            var msg = Factory.CreateNew<VesselSrvMsg>(msgData);

            //Serialize and compress
            var expectedDataSize = msg.GetMessageSize(false);
            var lidgrenMsgSend = client.CreateMessage(expectedDataSize);
            msg.Serialize(lidgrenMsgSend, false);
            var realSize = lidgrenMsgSend.LengthBytes;

            //Usually the expected size will be a bit more as Lidgren writes the size of the strings in a base128 int (so it uses less bytes)
            Assert.IsTrue(expectedDataSize >= realSize);

            //Simulate sending
            var data = lidgrenMsgSend.ReadBytes(lidgrenMsgSend.LengthBytes);
            var lidgrenMsgRecv = client.CreateIncomingMessage(NetIncomingMessageType.Data, data);
            lidgrenMsgRecv.LengthBytes = lidgrenMsgSend.LengthBytes;

            //Deserialize
            var msgDes = Factory.Deserialize(lidgrenMsgRecv, Environment.TickCount);

            //Arrays are pooled and will not be of an exact length so resize them
            Array.Resize(ref ((VesselProtoMsgData)msg.Data).Vessel.Data, 10000);
            Array.Resize(ref ((VesselProtoMsgData)msgDes.Data).Vessel.Data, 10000);

            Assert.IsTrue(((VesselProtoMsgData)msg.Data).Vessel.Data.SequenceEqual(((VesselProtoMsgData)msgDes.Data).Vessel.Data));
        }
    }
}
