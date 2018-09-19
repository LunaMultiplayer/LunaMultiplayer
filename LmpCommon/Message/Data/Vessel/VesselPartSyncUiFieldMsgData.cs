using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselPartSyncUiFieldMsgData : VesselBaseMsgData
    {
        internal VesselPartSyncUiFieldMsgData() { }

        public uint PartFlightId;
        public uint PartPersistentId;
        public string ModuleName;
        public string FieldName;

        public PartSyncFieldType FieldType;

        public bool BoolValue;
        public int IntValue;
        public float FloatValue;

        public override VesselMessageType VesselMessageType => VesselMessageType.PartSyncUiField;

        public override string ClassName { get; } = nameof(VesselPartSyncUiFieldMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PartFlightId);
            lidgrenMsg.Write(PartPersistentId);
            lidgrenMsg.Write(ModuleName);
            lidgrenMsg.Write(FieldName);

            lidgrenMsg.Write((byte)FieldType);

            switch (FieldType)
            {
                case PartSyncFieldType.Boolean:
                    lidgrenMsg.Write(BoolValue);
                    break;
                case PartSyncFieldType.Integer:
                    lidgrenMsg.Write(IntValue);
                    break;
                case PartSyncFieldType.Float:
                    lidgrenMsg.Write(FloatValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PartFlightId = lidgrenMsg.ReadUInt32();
            PartPersistentId = lidgrenMsg.ReadUInt32();
            ModuleName = lidgrenMsg.ReadString();
            FieldName = lidgrenMsg.ReadString();

            FieldType = (PartSyncFieldType)lidgrenMsg.ReadByte();

            switch (FieldType)
            {
                case PartSyncFieldType.Boolean:
                    BoolValue = lidgrenMsg.ReadBoolean();
                    break;
                case PartSyncFieldType.Integer:
                    IntValue = lidgrenMsg.ReadInt32();
                    break;
                case PartSyncFieldType.Float:
                    FloatValue = lidgrenMsg.ReadFloat();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal override int InternalGetMessageSize()
        {
            var msgSize = base.InternalGetMessageSize() + sizeof(uint) * 2 + ModuleName.GetByteCount() + FieldName.GetByteCount();

            switch (FieldType)
            {
                case PartSyncFieldType.Boolean:
                    msgSize += sizeof(bool);
                    break;
                case PartSyncFieldType.Integer:
                    msgSize += sizeof(int);
                    break;
                case PartSyncFieldType.Float:
                    msgSize += sizeof(float);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return msgSize;
        }
    }
}
