using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselsReplyMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselsReplyMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.VesselsReply;

        public int VesselsCount;
        public VesselInfo[] VesselsData = new VesselInfo[0];

        public override string ClassName { get; } = nameof(VesselsReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(VesselsCount);
            for (var i = 0; i < VesselsCount; i++)
            {
                VesselsData[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            VesselsCount = lidgrenMsg.ReadInt32();
            if (VesselsData.Length < VesselsCount)
                VesselsData = new VesselInfo[VesselsCount];
            
            for (var i = 0; i < VesselsCount; i++)
            {
                if (VesselsData[i] == null)
                    VesselsData[i] = new VesselInfo();

                VesselsData[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < VesselsCount; i++)
            {
                arraySize += VesselsData[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}