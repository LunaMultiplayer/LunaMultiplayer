using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselListReplyMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselListReplyMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.ListReply;

        public int VesselsCount;
        public string[] Vessels = new string[0];

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(VesselsCount);
            for (var i = 0; i < VesselsCount; i++)
            {
                lidgrenMsg.Write(Vessels[i]);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            VesselsCount = lidgrenMsg.ReadInt32();
            Vessels = ArrayPool<string>.Claim(VesselsCount);
            for (var i = 0; i < VesselsCount; i++)
            {
                Vessels[i] = lidgrenMsg.ReadString();
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            ArrayPool<string>.Release(ref Vessels);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + Vessels.GetByteCount(VesselsCount);
        }
    }
}