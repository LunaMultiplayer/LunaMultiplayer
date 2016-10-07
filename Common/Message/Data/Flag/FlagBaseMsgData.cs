using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)FlagMessageType;

        public virtual FlagMessageType FlagMessageType
        {
            get { throw new NotImplementedException(); }
        }

        public string PlayerName { get; set; }
    }
}