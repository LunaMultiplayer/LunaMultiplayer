using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorReplyMsgData : PlayerColorBaseMsgData
    {
        public override PlayerColorMessageType PlayerColorMessageType => PlayerColorMessageType.REPLY;
        public int Count { get; set; }

        /// <summary>
        ///     Color in HTML format
        /// </summary>
        public KeyValuePair<string, string>[] PlayersColors { get; set; }
    }
}