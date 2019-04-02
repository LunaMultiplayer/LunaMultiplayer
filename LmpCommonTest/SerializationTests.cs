using Lidgren.Network;
using LmpCommon.Message;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Chat;
using LmpCommon.Message.Data.Kerbal;
using LmpCommon.Message.Data.Vessel;
using LmpCommonTest.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LmpCommonTest
{
    [TestClass]
    public class SerializationTests
    {
        private static readonly ServerMessageFactory Factory = new ServerMessageFactory();
        private static readonly Random Rnd = new Random();
        private static readonly NetClient Client = new NetClient(new NetPeerConfiguration("TESTS"));

        [TestMethod]
        public void TestSerializeDeserializeVesselUpdateMsg()
        {
            var msgData = Factory.CreateNewMessageData<VesselUpdateMsgData>();
            msgData.VesselId = Guid.NewGuid();
            msgData.Name = "Name";
            msgData.Type = "Type";
            msgData.DistanceTraveled = 222;
            msgData.Situation = "Situation";
            msgData.Landed = true;
            msgData.LandedAt = "LandedAt";
            msgData.DisplayLandedAt = "DisplayLandedAt";
            msgData.Splashed = false;
            msgData.MissionTime = Rnd.NextDouble();
            msgData.LaunchTime = Rnd.NextDouble();
            msgData.LastUt = Rnd.NextDouble();
            msgData.Persistent = false;
            msgData.RefTransformId = (uint)Rnd.Next();

            var msg = Factory.CreateNew<VesselCliMsg>(msgData);

            //Serialize
            var expectedDataSize = msg.GetMessageSize();
            var lidgrenMsgSend = Client.CreateMessage(expectedDataSize);
            msg.Serialize(lidgrenMsgSend);
            var realSize = lidgrenMsgSend.LengthBytes;

            //Usually the expected size will be a bit more as Lidgren writes the size of the strings in a base128 int (so it uses less bytes)
            Assert.IsTrue(expectedDataSize >= realSize);

            //Simulate sending
            var data = lidgrenMsgSend.ReadBytes(lidgrenMsgSend.LengthBytes);
            var lidgrenMsgRecv = Client.CreateIncomingMessage(NetIncomingMessageType.Data, data);
            lidgrenMsgRecv.LengthBytes = lidgrenMsgSend.LengthBytes;

            msg.Recycle();

            //Deserialize
            var msgDes = Factory.Deserialize(lidgrenMsgRecv, Environment.TickCount);

        }

        [TestMethod]
        public void TestSerializeDeserializeChatChannelMsg()
        {
            var msgData = Factory.CreateNewMessageData<ChatMsgData>();
            msgData.Text = "T";

            var msg = Factory.CreateNew<ChatCliMsg>(msgData);

            //Serialize
            var expectedDataSize = msg.GetMessageSize();
            var lidgrenMsgSend = Client.CreateMessage(expectedDataSize);
            msg.Serialize(lidgrenMsgSend);
            var realSize = lidgrenMsgSend.LengthBytes;

            //Usually the expected size will be a bit more as Lidgren writes the size of the strings in a base128 int (so it uses less bytes)
            Assert.IsTrue(expectedDataSize >= realSize);

            //Simulate sending
            var data = lidgrenMsgSend.ReadBytes(lidgrenMsgSend.LengthBytes);
            var lidgrenMsgRecv = Client.CreateIncomingMessage(NetIncomingMessageType.Data, data);
            lidgrenMsgRecv.LengthBytes = lidgrenMsgSend.LengthBytes;

            msg.Recycle();

            //Deserialize
            var msgDes = Factory.Deserialize(lidgrenMsgRecv, Environment.TickCount);
        }

        [TestMethod]
        public void TestSerializeCompressThreadSafety()
        {
            var numThreads = 100000;

            var msgData = Factory.CreateNewMessageData<KerbalProtoMsgData>();
            msgData.Kerbal.KerbalName = "TEST";
            msgData.Kerbal.KerbalData = Encoding.UTF8.GetBytes(Resources.Jebediah_Kerman);
            msgData.Kerbal.NumBytes = msgData.Kerbal.KerbalData.Length;

            var msg = Factory.CreateNew<KerbalCliMsg>(msgData);

            var taskPool = new List<Task<bool>>();

            for (var i = 0; i < numThreads; i++)
            {
                taskPool.Add(new Task<bool>(() =>
                {
                    try
                    {
                        msg.Serialize(Client.CreateMessage());
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    return true;
                }));
            }

            Parallel.ForEach(taskPool, t => t.Start());
            // ReSharper disable once CoVariantArrayConversion
            Task.WaitAll(taskPool.ToArray());

            for (var i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(taskPool[i].Result, "Error while serializing in parallel!");
            }
        }
    }
}
