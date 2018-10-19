using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselDockMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselDockMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Dock;

        public int SubspaceId;

        public Guid DominantVesselId;
        public Guid WeakVesselId;

        public int NumBytes;
        public byte[] FinalVesselData = new byte[0];

        public override string ClassName { get; } = nameof(VesselDockMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(SubspaceId);
            GuidUtil.Serialize(DominantVesselId, lidgrenMsg);
            GuidUtil.Serialize(WeakVesselId, lidgrenMsg);

            Common.ThreadSafeCompress(this, ref FinalVesselData, ref NumBytes);

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(FinalVesselData, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            SubspaceId = lidgrenMsg.ReadInt32();
            DominantVesselId = GuidUtil.Deserialize(lidgrenMsg);
            WeakVesselId = GuidUtil.Deserialize(lidgrenMsg);

            NumBytes = lidgrenMsg.ReadInt32();
            if (FinalVesselData.Length < NumBytes)
                FinalVesselData = new byte[NumBytes];

            lidgrenMsg.ReadBytes(FinalVesselData, 0, NumBytes);

            Common.ThreadSafeDecompress(this, ref FinalVesselData, NumBytes, out NumBytes);
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) + 
                GuidUtil.ByteSize * 2 + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
