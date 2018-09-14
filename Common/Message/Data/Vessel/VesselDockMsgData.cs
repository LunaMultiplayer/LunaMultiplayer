using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselDockMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselDockMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Dock;

        public int SubspaceId;

        public Guid DominantVesselId;
        public uint DominantVesselPersistentId;
        public Guid WeakVesselId;
        public uint WeakVesselPersistentId;

        public int NumBytes;
        public byte[] FinalVesselData = new byte[0];

        public override string ClassName { get; } = nameof(VesselDockMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(SubspaceId);
            GuidUtil.Serialize(DominantVesselId, lidgrenMsg);
            lidgrenMsg.Write(DominantVesselPersistentId);
            GuidUtil.Serialize(WeakVesselId, lidgrenMsg);
            lidgrenMsg.Write(WeakVesselPersistentId);

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(FinalVesselData, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            SubspaceId = lidgrenMsg.ReadInt32();
            DominantVesselId = GuidUtil.Deserialize(lidgrenMsg);
            DominantVesselPersistentId = lidgrenMsg.ReadUInt32();
            WeakVesselId = GuidUtil.Deserialize(lidgrenMsg);
            WeakVesselPersistentId = lidgrenMsg.ReadUInt32();

            NumBytes = lidgrenMsg.ReadInt32();
            if (FinalVesselData.Length < NumBytes)
                FinalVesselData = new byte[NumBytes];

            lidgrenMsg.ReadBytes(FinalVesselData, 0, NumBytes);
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) + 
                GuidUtil.ByteSize * 2 + sizeof(uint) * 2 + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
