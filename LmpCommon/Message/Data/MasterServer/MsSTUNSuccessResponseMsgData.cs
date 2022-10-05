using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System.Net;

namespace LmpCommon.Message.Data.MasterServer
{
    public class MsSTUNSuccessResponseMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsSTUNSuccessResponseMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.STUNSuccessResponse;
        public IPEndPoint TransportAddress;

        public override string ClassName { get; } = nameof(MsRegisterServerMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(TransportAddress);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            TransportAddress = lidgrenMsg.ReadIPEndPoint();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + TransportAddress.GetByteCount();
        }
    }
}
