using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalsRequestMsgData : KerbalBaseMsgData
    {
        /// <inheritdoc />
        internal KerbalsRequestMsgData() { }
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Request;
    }
}