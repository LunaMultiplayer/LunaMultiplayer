using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Kerbal
{
    public class KerbalsRequestMsgData : KerbalBaseMsgData
    {
        /// <inheritdoc />
        internal KerbalsRequestMsgData() { }
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Request;

        public override string ClassName { get; } = nameof(KerbalsRequestMsgData);
    }
}