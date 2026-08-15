using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselProtoMsgData : VesselBaseMsgData
    {
        internal VesselProtoMsgData() { }

        public int NumBytes;
        public byte[] Data = new byte[0];
        public bool ForceReload;

        /// <summary>
        /// Human-readable description of why this vessel update is being sent
        /// (e.g. "Flight ready (launch)", "Part decoupled", "Science transmission").
        /// Purely informational; consumed by the server for the craft-create/remove audit log.
        /// Written at the END of the payload so older peers (who stop reading earlier) stay compatible.
        /// </summary>
        public string Reason;

        public override VesselMessageType VesselMessageType => VesselMessageType.Proto;

        public override string ClassName { get; } = nameof(VesselProtoMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(ForceReload);
            Common.ThreadSafeCompress(this, ref Data, ref NumBytes);

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);

            // Backwards-compatible field: must be written LAST so older peers that don't know
            // about it simply stop reading before this byte range.
            lidgrenMsg.Write(Reason ?? string.Empty);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            ForceReload = lidgrenMsg.ReadBoolean();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);

            Common.ThreadSafeDecompress(this, ref Data, NumBytes, out NumBytes);

            // Backwards-compatible read: older peers don't send this trailing field.
            Reason = lidgrenMsg.Position < lidgrenMsg.LengthBits ? lidgrenMsg.ReadString() : null;
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(bool) + sizeof(int) + sizeof(byte) * NumBytes + Reason.GetByteCount();
        }
    }
}
