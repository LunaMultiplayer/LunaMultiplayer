using System;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerConnection
{
    public class PlayerConnectionBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)PlayerConnectionMessageType;
        public string PlayerName { get; set; }

        public virtual PlayerConnectionMessageType PlayerConnectionMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
