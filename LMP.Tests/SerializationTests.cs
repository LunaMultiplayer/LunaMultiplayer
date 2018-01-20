using Lidgren.Network;
using LunaCommon.Message;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using LunaCommon.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using LunaCommon.Message.Data.Chat;

namespace LMP.Tests
{
    [TestClass]
    public class SerializationTests
    {
        private static readonly ServerMessageFactory Factory = new ServerMessageFactory();
        private static readonly Random Rnd = new Random();
        private static readonly NetClient Client = new NetClient(new NetPeerConfiguration("TESTS"));

        [TestMethod]
        public void TestSerializeDeserialize()
        {
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
            var lidgrenMsgSend = Client.CreateMessage(expectedDataSize);
            msg.Serialize(lidgrenMsgSend, false);
            var realSize = lidgrenMsgSend.LengthBytes;

            //Usually the expected size will be a bit more as Lidgren writes the size of the strings in a base128 int (so it uses less bytes)
            Assert.IsTrue(expectedDataSize >= realSize);

            //Simulate sending
            var data = lidgrenMsgSend.ReadBytes(lidgrenMsgSend.LengthBytes);
            var lidgrenMsgRecv = Client.CreateIncomingMessage(NetIncomingMessageType.Data, data);
            lidgrenMsgRecv.LengthBytes = lidgrenMsgSend.LengthBytes;

            //Deserialize
            var msgDes = Factory.Deserialize(lidgrenMsgRecv, Environment.TickCount);

            //Arrays are pooled and will not be of an exact length so resize them
            Array.Resize(ref ((VesselProtoMsgData)msg.Data).Vessel.Data, 10000);
            Array.Resize(ref ((VesselProtoMsgData)msgDes.Data).Vessel.Data, 10000);

            Assert.IsTrue(((VesselProtoMsgData)msg.Data).Vessel.Data.SequenceEqual(((VesselProtoMsgData)msgDes.Data).Vessel.Data));
        }

        [TestMethod]
        public void TestSerializeDeserializeVesselUpdateMsg()
        {
            var msgData = Factory.CreateNewMessageData<VesselUpdateMsgData>();
            msgData.VesselId = Guid.NewGuid();
            msgData.Name = "Name";
            msgData.Type = "Type";
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
            msgData.Controllable = true;

            for (var i = 0; i < 17; i++)
            {
                if (msgData.ActionGroups[i] == null)
                    msgData.ActionGroups[i] = new ActionGroup();

                msgData.ActionGroups[i].ActionGroupName = "ActionGroupName" + i;
                msgData.ActionGroups[i].State = true;
                msgData.ActionGroups[i].Time = Rnd.NextDouble();
            }

            var msg = Factory.CreateNew<VesselCliMsg>(msgData);
            
            //Serialize and compress
            var expectedDataSize = msg.GetMessageSize(false);
            var lidgrenMsgSend = Client.CreateMessage(expectedDataSize);
            msg.Serialize(lidgrenMsgSend, false);
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
            var msgData = Factory.CreateNewMessageData<ChatChannelMsgData>();
            msgData.Channel = "C";
            msgData.SendToAll = true;
            msgData.Text = "T";

            var msg = Factory.CreateNew<ChatCliMsg>(msgData);

            //Serialize and compress
            var expectedDataSize = msg.GetMessageSize(false);
            var lidgrenMsgSend = Client.CreateMessage(expectedDataSize);
            msg.Serialize(lidgrenMsgSend, false);
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
    }
}
