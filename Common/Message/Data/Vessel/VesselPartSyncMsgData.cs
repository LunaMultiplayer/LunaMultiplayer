using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselPartSyncMsgData : VesselBaseMsgData
    {
        internal VesselPartSyncMsgData() { }

        public Guid VesselId;

        public int NumParts;
        public PartSync[] Parts = new PartSync[0];

        public override VesselMessageType VesselMessageType => VesselMessageType.PartSync;

        public override string ClassName { get; } = nameof(VesselPartSyncMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            GuidUtil.Serialize(VesselId, lidgrenMsg);

            lidgrenMsg.Write(NumParts);
            for (var i = 0; i < NumParts; i++)
            {
                Parts[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            VesselId = GuidUtil.Deserialize(lidgrenMsg);

            NumParts = lidgrenMsg.ReadInt32();
            if (Parts.Length < NumParts)
                Parts = new PartSync[NumParts];

            for (var i = 0; i < NumParts; i++)
            {
                if (Parts[i] == null)
                    Parts[i] = new PartSync();

                Parts[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < NumParts; i++)
            {
                arraySize += Parts[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + GuidUtil.GetByteSize() + sizeof(int) + arraySize;
        }
    }
}
