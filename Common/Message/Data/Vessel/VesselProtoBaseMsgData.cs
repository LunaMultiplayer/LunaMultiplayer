using Lidgren.Network;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselProtoBaseMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselProtoBaseMsgData() { }

        public int SubspaceId;
        public VesselInfo Vessel = new VesselInfo();

        public override string ClassName { get; } = nameof(VesselProtoBaseMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(SubspaceId);
            Vessel.Serialize(lidgrenMsg, dataCompressed);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            SubspaceId = lidgrenMsg.ReadInt32();
            Vessel.Deserialize(lidgrenMsg, dataCompressed);
        }

        public override void Recycle()
        {
            base.Recycle();

            Vessel.Recycle();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + Vessel.GetByteCount(dataCompressed);
        }
    }
}