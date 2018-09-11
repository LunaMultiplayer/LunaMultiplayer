using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselPartSyncMsgData : VesselBaseMsgData
    {
        internal VesselPartSyncMsgData() { }

        public uint PartFlightId;
        public string ModuleName;
        public string MethodName;

        public int FieldCount;
        public FieldNameValue[] FieldValues = new FieldNameValue[0];

        public bool IsAction;
        public int ActionGroup;
        public int ActionType;

        public override VesselMessageType VesselMessageType => VesselMessageType.PartSync;

        public override string ClassName { get; } = nameof(VesselPartSyncMsgData);

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

            lidgrenMsg.Write(IsAction);
            lidgrenMsg.Write(ActionGroup);
            lidgrenMsg.Write(ActionType);
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

            IsAction = lidgrenMsg.ReadBoolean();
            ActionGroup = lidgrenMsg.ReadInt32();
            ActionType = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < FieldCount; i++)
            {
                arraySize += FieldValues[i]?.GetByteCount() ?? 0;
            }

            return base.InternalGetMessageSize() + sizeof(uint) + ModuleName.GetByteCount() + MethodName.GetByteCount() + sizeof(int) * 3 + arraySize + sizeof(bool);
        }
    }
}
