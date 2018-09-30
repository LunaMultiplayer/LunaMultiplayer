using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselProtoMsgData : VesselBaseMsgData
    {
        internal VesselProtoMsgData() { }
        public override bool CompressCondition => true;

        public int NumBytes;
        public byte[] Data = new byte[0];

        public override VesselMessageType VesselMessageType => VesselMessageType.Proto;

        public override string ClassName { get; } = nameof(VesselProtoMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            
            if (CompressCondition)
            {
                var compressedData = CachedQuickLz.CachedQlz.Compress(Data, NumBytes, out NumBytes);
                CachedQuickLz.ArrayPool<byte>.Recycle(Data);
                Data = compressedData;
            }
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
            if (CompressCondition)
            {
                CachedQuickLz.ArrayPool<byte>.Recycle(Data);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);

            if (Compressed)
            {
                var decompressedData = CachedQuickLz.CachedQlz.Decompress(Data, out NumBytes);
                CachedQuickLz.ArrayPool<byte>.Recycle(Data);
                Data = decompressedData;
            }
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
