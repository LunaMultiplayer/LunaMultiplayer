using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.PlayerConnection
{
    public class PlayerConnectionBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)PlayerConnectionMessageType;
        public string PlayerName { get; set; }

        public virtual PlayerConnectionMessageType PlayerConnectionMessageType => throw new NotImplementedException();
    }
}
