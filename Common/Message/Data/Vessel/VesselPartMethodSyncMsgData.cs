using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselPartMethodSyncMsgData : VesselBaseMsgData
    {
        internal VesselPartMethodSyncMsgData() { }

        public uint PartFlightId;
        public string ModuleName;
        public string MethodName;

        public int FieldCount;
        public FieldNameValue[] FieldValues = new FieldNameValue[0];

        public override VesselMessageType VesselMessageType => VesselMessageType.PartMethodSync;

        public override string ClassName { get; } = nameof(VesselPartMethodSyncMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PartFlightId);
            lidgrenMsg.Write(ModuleName);
            lidgrenMsg.Write(MethodName);

            lidgrenMsg.Write(FieldCount);
            for (var i = 0; i < FieldCount; i++)
            {
                FieldValues[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PartFlightId = lidgrenMsg.ReadUInt32();
            ModuleName = lidgrenMsg.ReadString();
            MethodName = lidgrenMsg.ReadString();

            FieldCount = lidgrenMsg.ReadInt32();
            if (FieldValues.Length < FieldCount)
                FieldValues = new FieldNameValue[FieldCount];

            for (var i = 0; i < FieldCount; i++)
            {
                if (FieldValues[i] == null)
                    FieldValues[i] = new FieldNameValue();

                FieldValues[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < FieldCount; i++)
            {
                arraySize += FieldValues[i]?.GetByteCount() ?? 0;
            }

            return base.InternalGetMessageSize() + sizeof(uint) + ModuleName.GetByteCount() + MethodName.GetByteCount() + sizeof(int) + arraySize;
        }
    }
}
