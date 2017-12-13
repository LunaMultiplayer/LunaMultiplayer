using LunaCommon.Message;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using LunaCommon.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Lidgren.Network;

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
            msgData.VesselId = Guid.NewGuid();
            msgData.VesselData = bytes;
            msgData.SentTime = LunaTime.UtcNow.Ticks;

            var msg = Factory.CreateNew<VesselSrvMsg>(msgData);

            //Serialize and compress
            var serialized = msg.Serialize(true, out var totalLength);

            //Simulate sending
            serialized = serialized.Take(totalLength).ToArray();
            var lidgrenMsg1 = client.CreateIncomingMessage(NetIncomingMessageType.Data, serialized);
            lidgrenMsg1.LengthBytes = serialized.Length;

            //Serialize no compress
            var serializedNc = msg.Serialize(false, out totalLength);

            //Simulate sending
            serializedNc = serializedNc.Take(totalLength).ToArray();
            var lidgrenMsg2 = client.CreateIncomingMessage(NetIncomingMessageType.Data, serializedNc);
            lidgrenMsg2.LengthBytes = serializedNc.Length;

            //Deserialize compressedMsg
            var msg2 = Factory.Deserialize(lidgrenMsg1, Environment.TickCount);
            //Deserialize no compressed message
            var msg2Nc = Factory.Deserialize(lidgrenMsg2, Environment.TickCount);

           Assert.IsTrue(((VesselProtoMsgData)msg.Data).VesselData.SequenceEqual(((VesselProtoMsgData)msg2.Data).VesselData) &&
                     ((VesselProtoMsgData)msg2Nc.Data).VesselData.SequenceEqual(((VesselProtoMsgData)msg2.Data).VesselData));
        }
    }
}
