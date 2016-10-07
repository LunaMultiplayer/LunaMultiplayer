using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)PlayerStatusMessageType;

        public virtual PlayerStatusMessageType PlayerStatusMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
