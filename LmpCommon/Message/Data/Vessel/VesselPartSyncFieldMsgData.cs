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
        public string ModuleName;
        public string FieldName;

        public PartSyncFieldType FieldType;

        public string StrValue;
        public bool BoolValue;
        public short ShortValue;
        public ushort UShortValue;
        public int IntValue;
        public uint UIntValue;
        public long LongValue;
        public ulong ULongValue;
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
            lidgrenMsg.Write(ModuleName);
            lidgrenMsg.Write(FieldName);

            lidgrenMsg.Write((byte)FieldType);

            switch (FieldType)
            {
                case PartSyncFieldType.Boolean:
                    lidgrenMsg.Write(BoolValue);
                    break;
                case PartSyncFieldType.Short:
                    lidgrenMsg.Write(ShortValue);
                    break;
                case PartSyncFieldType.UShort:
                    lidgrenMsg.Write(UShortValue);
                    break;
                case PartSyncFieldType.Integer:
                    lidgrenMsg.Write(IntValue);
                    break;
                case PartSyncFieldType.UInteger:
                    lidgrenMsg.Write(UIntValue);
                    break;
                case PartSyncFieldType.Float:
                    lidgrenMsg.Write(FloatValue);
                    break;
                case PartSyncFieldType.Long:
                    lidgrenMsg.Write(LongValue);
                    break;
                case PartSyncFieldType.ULong:
                    lidgrenMsg.Write(ULongValue);
                    break;
                case PartSyncFieldType.Double:
                    lidgrenMsg.Write(DoubleValue);
                    break;
                case PartSyncFieldType.Vector2:
                    for (var i = 0; i < 2; i++)
                        lidgrenMsg.Write(VectorValue[i]);
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
            ModuleName = lidgrenMsg.ReadString();
            FieldName = lidgrenMsg.ReadString();

            FieldType = (PartSyncFieldType)lidgrenMsg.ReadByte();

            switch (FieldType)
            {
                case PartSyncFieldType.Boolean:
                    BoolValue = lidgrenMsg.ReadBoolean();
                    break;
                case PartSyncFieldType.Short:
                    ShortValue = lidgrenMsg.ReadInt16();
                    break;
                case PartSyncFieldType.UShort:
                    UShortValue = lidgrenMsg.ReadUInt16();
                    break;
                case PartSyncFieldType.Integer:
                    IntValue = lidgrenMsg.ReadInt32();
                    break;
                case PartSyncFieldType.UInteger:
                    UIntValue = lidgrenMsg.ReadUInt32();
                    break;
                case PartSyncFieldType.Float:
                    FloatValue = lidgrenMsg.ReadFloat();
                    break;
                case PartSyncFieldType.Long:
                    LongValue = lidgrenMsg.ReadInt64();
                    break;
                case PartSyncFieldType.ULong:
                    ULongValue = lidgrenMsg.ReadUInt64();
                    break;
                case PartSyncFieldType.Double:
                    DoubleValue = lidgrenMsg.ReadDouble();
                    break;
                case PartSyncFieldType.Vector2:
                    for (var i = 0; i < 2; i++)
                        VectorValue[i] = lidgrenMsg.ReadFloat();
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
            var msgSize = base.InternalGetMessageSize() + sizeof(uint) + ModuleName.GetByteCount() + FieldName.GetByteCount();

            switch (FieldType)
            {
                case PartSyncFieldType.Boolean:
                    msgSize += sizeof(bool);
                    break;
                case PartSyncFieldType.Short:
                    msgSize += sizeof(short);
                    break;
                case PartSyncFieldType.UShort:
                    msgSize += sizeof(ushort);
                    break;
                case PartSyncFieldType.Integer:
                    msgSize += sizeof(int);
                    break;
                case PartSyncFieldType.UInteger:
                    msgSize += sizeof(uint);
                    break;
                case PartSyncFieldType.Float:
                    msgSize += sizeof(float);
                    break;
                case PartSyncFieldType.Long:
                    msgSize += sizeof(long);
                    break;
                case PartSyncFieldType.ULong:
                    msgSize += sizeof(ulong);
                    break;
                case PartSyncFieldType.Double:
                    msgSize += sizeof(double);
                    break;
                case PartSyncFieldType.Vector2:
                    msgSize += sizeof(float) * 2;
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
