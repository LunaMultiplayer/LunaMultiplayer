using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)AdminMessageType;

        public virtual AdminMessageType AdminMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}