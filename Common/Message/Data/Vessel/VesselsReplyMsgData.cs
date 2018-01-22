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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

            lidgrenMsg.Write(VesselsCount);
            for (var i = 0; i < VesselsCount; i++)
            {
                VesselsData[i].Serialize(lidgrenMsg, compressData);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            VesselsCount = lidgrenMsg.ReadInt32();
            if (VesselsData.Length < VesselsCount)
                VesselsData = new VesselInfo[VesselsCount];
            
            for (var i = 0; i < VesselsCount; i++)
            {
                if (VesselsData[i] == null)
                    VesselsData[i] = new VesselInfo();

                VesselsData[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < VesselsCount; i++)
            {
                arraySize += VesselsData[i].GetByteCount(dataCompressed);
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}