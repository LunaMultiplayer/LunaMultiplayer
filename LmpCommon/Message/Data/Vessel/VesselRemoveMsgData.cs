using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselRemoveMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselRemoveMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Remove;

        public bool AddToKillList;
        public bool KillOnReceive;

        /// <summary>
        /// Human-readable description of why this vessel is being removed
        /// (e.g. "Revert to VAB", "Terminated", "Destroyed", "Coupled/Docked").
        /// Purely informational; consumed by the server for the craft-create/remove audit log.
        /// Written at the END of the payload so older peers (who stop reading earlier) stay compatible.
        /// </summary>
        public string Reason;

        public override string ClassName { get; } = nameof(VesselRemoveMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(AddToKillList);
            lidgrenMsg.Write(KillOnReceive);

            // Backwards-compatible field: must be written LAST so older peers that don't know
            // about it simply stop reading before this byte range.
            lidgrenMsg.Write(Reason ?? string.Empty);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            AddToKillList = lidgrenMsg.ReadBoolean();
            KillOnReceive = lidgrenMsg.ReadBoolean();

            // Backwards-compatible read: older peers don't send this trailing field.
            Reason = lidgrenMsg.Position < lidgrenMsg.LengthBits ? lidgrenMsg.ReadString() : null;
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(bool) * 2 + Reason.GetByteCount();
        }
    }
}
