using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System.Net;

namespace LmpCommon.Message.Data.MasterServer
{
    public class MsIntroductionMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsIntroductionMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.Introduction;

        public long Id;
        public IPEndPoint InternalEndpoint;
        public IPEndPoint InternalEndpoint6;
        public string Token;

        public override string ClassName { get; } = nameof(MsIntroductionMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Id);
            lidgrenMsg.Write(InternalEndpoint);
            lidgrenMsg.Write(InternalEndpoint6);
            lidgrenMsg.Write(Token);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Id = lidgrenMsg.ReadInt64();
            InternalEndpoint = lidgrenMsg.ReadIPEndPoint();
            // ReadIPEndPoint supports IPv6 addresses despite saying otherwise in the code doc.
            InternalEndpoint6 = lidgrenMsg.ReadIPEndPoint();
            Token = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(long) + InternalEndpoint.GetByteCount() +
                   InternalEndpoint6.GetByteCount() + Token.GetByteCount();
        }
    }
}
