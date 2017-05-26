using System.Collections.Generic;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalReplyMsgData : KerbalBaseMsgData
    {
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Reply;
        public KeyValuePair<string, byte[]>[] KerbalsData { get; set; }
    }
}