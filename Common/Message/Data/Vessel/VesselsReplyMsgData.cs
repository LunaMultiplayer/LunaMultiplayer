using Lidgren.Network;
using LunaCommon.Message.Base;
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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(VesselsCount);
            for (var i = 0; i < VesselsCount; i++)
            {
                VesselsData[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            VesselsCount = lidgrenMsg.ReadInt32();
            VesselsData = ArrayPool<VesselInfo>.Claim(VesselsCount);
            for (var i = 0; i < VesselsCount; i++)
            {
                if (VesselsData[i] == null)
                    VesselsData[i] = new VesselInfo();

                VesselsData[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            for (var i = 0; i < VesselsCount; i++)
            {
                VesselsData[i].Recycle();
            }
            ArrayPool<VesselInfo>.Release(ref VesselsData);
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