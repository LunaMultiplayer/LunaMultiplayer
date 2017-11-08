using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalReplyMsgData : KerbalBaseMsgData
    {
        /// <inheritdoc />
        internal KerbalReplyMsgData() { }
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Reply;
        public KeyValuePair<string, byte[]>[] KerbalsData { get; set; }
    }
}