using CachedQuickLz;
using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselProtoMsgData : VesselBaseMsgData
    {
        internal VesselProtoMsgData() { }

        public int NumBytes;
        public byte[] Data = new byte[0];

        public override VesselMessageType VesselMessageType => VesselMessageType.Proto;

        public override string ClassName { get; } = nameof(VesselProtoMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            
            Common.ThreadSafeCompress(this, ref Data, ref NumBytes);

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);

            CachedQlz.Decompress(ref Data, out NumBytes);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
