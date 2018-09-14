using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselPersistentMsgData : VesselBaseMsgData
    {
        internal VesselPersistentMsgData() { }

        public bool PartPersistentChange;
        public uint From;
        public uint To;

        public override VesselMessageType VesselMessageType => VesselMessageType.PartSyncField;

        public override string ClassName { get; } = nameof(VesselPersistentMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(From);
            lidgrenMsg.Write(To);
            lidgrenMsg.Write(PartPersistentChange);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            From = lidgrenMsg.ReadUInt32();
            To = lidgrenMsg.ReadUInt32();
            PartPersistentChange = lidgrenMsg.ReadBoolean();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(bool) + sizeof(uint) * 2;
        }
    }
}
