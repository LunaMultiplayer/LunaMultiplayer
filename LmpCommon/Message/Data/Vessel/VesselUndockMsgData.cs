using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselUndockMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselUndockMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Undock;

        public uint PartFlightId;
        public Guid NewVesselId;

        public string DockedInfoName;
        public uint DockedInfoRootPartUId;
        public int DockedInfoVesselType;

        public override string ClassName { get; } = nameof(VesselUndockMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PartFlightId);
            GuidUtil.Serialize(NewVesselId, lidgrenMsg);
            lidgrenMsg.Write(DockedInfoName);
            lidgrenMsg.Write(DockedInfoRootPartUId);
            lidgrenMsg.Write(DockedInfoVesselType);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PartFlightId = lidgrenMsg.ReadUInt32();
            NewVesselId = GuidUtil.Deserialize(lidgrenMsg);
            DockedInfoName = lidgrenMsg.ReadString();
            DockedInfoRootPartUId = lidgrenMsg.ReadUInt32();
            DockedInfoVesselType = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(uint) * 2 + sizeof(int) + GuidUtil.ByteSize + DockedInfoName.GetByteCount();
        }
    }
}
