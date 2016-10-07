using System;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Motd
{
    public class MotdBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)MotdMessageType;

        public virtual MotdMessageType MotdMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
