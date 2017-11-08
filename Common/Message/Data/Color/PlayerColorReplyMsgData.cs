using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorReplyMsgData : PlayerColorBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerColorReplyMsgData() { }
        public override PlayerColorMessageType PlayerColorMessageType => PlayerColorMessageType.Reply;

        /// <summary>
        ///     Color in HTML format
        /// </summary>
        public KeyValuePair<string, string>[] PlayersColors { get; set; }
    }
}