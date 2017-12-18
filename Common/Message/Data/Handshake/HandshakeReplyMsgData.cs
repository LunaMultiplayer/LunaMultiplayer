using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeReplyMsgData : HandshakeBaseMsgData
    {
        /// <inheritdoc />
        internal HandshakeReplyMsgData() { }
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.Reply;

        public HandshakeReply Response;
        public string Reason;
        public ModControlMode ModControlMode;
        public long ServerStartTime;
        public Guid  PlayerId;
        public int NumBytes;
        public byte[] ModFileData = new byte[0];

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write((int)Response);
            lidgrenMsg.Write(Reason);
            lidgrenMsg.Write((int)ModControlMode);
            lidgrenMsg.Write(ServerStartTime);

            GuidUtil.Serialize(PlayerId, lidgrenMsg);

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(ModFileData, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            Response = (HandshakeReply)lidgrenMsg.ReadInt32();
            Reason = lidgrenMsg.ReadString();
            ModControlMode = (ModControlMode)lidgrenMsg.ReadInt32();
            ServerStartTime = lidgrenMsg.ReadInt64();
            
            PlayerId = GuidUtil.Deserialize(lidgrenMsg);

            NumBytes = lidgrenMsg.ReadInt32();
            ModFileData = ArrayPool<byte>.Claim(NumBytes);
            lidgrenMsg.ReadBytes(ModFileData, 0, NumBytes);
        }

        public override void Recycle()
        {
            base.Recycle();

            ArrayPool<byte>.Release(ref ModFileData);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(HandshakeReply) + Reason.GetByteCount() + sizeof(ModControlMode)
                + sizeof(long) + GuidUtil.GetByteSize() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}