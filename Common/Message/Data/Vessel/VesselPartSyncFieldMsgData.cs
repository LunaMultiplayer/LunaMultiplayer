using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselPartSyncFieldMsgData : VesselBaseMsgData
    {
        internal VesselPartSyncFieldMsgData() { }

        public uint PartFlightId;
        public string ModuleName;
        public string FieldName;
        public string Value;

        public override VesselMessageType VesselMessageType => VesselMessageType.PartSyncField;

        public override string ClassName { get; } = nameof(VesselPartSyncFieldMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PartFlightId);
            lidgrenMsg.Write(ModuleName);
            lidgrenMsg.Write(FieldName);
            lidgrenMsg.Write(Value);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PartFlightId = lidgrenMsg.ReadUInt32();
            ModuleName = lidgrenMsg.ReadString();
            FieldName = lidgrenMsg.ReadString();
            Value = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(uint) + ModuleName.GetByteCount() + FieldName.GetByteCount() + Value.GetByteCount();
        }
    }
}
