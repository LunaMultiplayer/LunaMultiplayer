using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselsRequestMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselsRequestMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.VesselsRequest;

        public int RequestCount;
        public string[] RequestList = new string[0];

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(RequestCount);
            for (var i = 0; i < RequestCount; i++)
            {
                lidgrenMsg.Write(RequestList[i]);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            RequestCount = lidgrenMsg.ReadInt32();
            RequestList = ArrayPool<string>.Claim(RequestCount);
            for (var i = 0; i < RequestCount; i++)
            {
                RequestList[i] = lidgrenMsg.ReadString();
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            ArrayPool<string>.Release(ref RequestList);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + RequestList.GetByteCount(RequestCount);
        }
    }
}