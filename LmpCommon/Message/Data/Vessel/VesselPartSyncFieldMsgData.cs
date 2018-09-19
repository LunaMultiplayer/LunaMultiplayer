using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselPartSyncFieldMsgData : VesselBaseMsgData
    {
        internal VesselPartSyncFieldMsgData() { }

        public uint PartFlightId;
        public uint PartPersistentId;
        public string ModuleName;
        public string FieldName;

        public PartSyncFieldType FieldType;

        public string StrValue;
        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public double DoubleValue;
        public float[] VectorValue = new float[3];
        public float[] QuaternionValue = new float[4];

        public override VesselMessageType VesselMessageType => VesselMessageType.PartSyncField;

        public override string ClassName { get; } = nameof(VesselPartSyncFieldMsgData);

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
                case PartSyncFieldType.Double:
                    lidgrenMsg.Write(DoubleValue);
                    break;
                case PartSyncFieldType.Vector3:
                    for (var i = 0; i < 3; i++)
                        lidgrenMsg.Write(VectorValue[i]);
                    break;
                case PartSyncFieldType.Quaternion:
                    for (var i = 0; i < 4; i++)
                        lidgrenMsg.Write(QuaternionValue[i]);
                    break;
                case PartSyncFieldType.Object:
                case PartSyncFieldType.String:
                    lidgrenMsg.Write(StrValue);
                    break;
                case PartSyncFieldType.Enum:
                    lidgrenMsg.Write(IntValue);
                    lidgrenMsg.Write(StrValue);
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
                case PartSyncFieldType.Double:
                    DoubleValue = lidgrenMsg.ReadDouble();
                    break;
                case PartSyncFieldType.Vector3:
                    for (var i = 0; i < 3; i++)
                        VectorValue[i] = lidgrenMsg.ReadFloat();
                    break;
                case PartSyncFieldType.Quaternion:
                    for (var i = 0; i < 4; i++)
                        VectorValue[i] = lidgrenMsg.ReadFloat();
                    break;
                case PartSyncFieldType.Object:
                case PartSyncFieldType.String:
                    StrValue = lidgrenMsg.ReadString();
                    break;
                case PartSyncFieldType.Enum:
                    IntValue = lidgrenMsg.ReadInt32();
                    StrValue = lidgrenMsg.ReadString();
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
                case PartSyncFieldType.Double:
                    msgSize += sizeof(double);
                    break;
                case PartSyncFieldType.Vector3:
                    msgSize += sizeof(float) * 3;
                    break;
                case PartSyncFieldType.Quaternion:
                    msgSize += sizeof(float) * 4;
                    break;
                case PartSyncFieldType.String:
                    msgSize += StrValue.GetByteCount();
                    break;
                case PartSyncFieldType.Enum:
                    msgSize += sizeof(int) + StrValue.GetByteCount();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return msgSize;
        }
    }
}
