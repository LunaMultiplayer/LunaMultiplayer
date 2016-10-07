using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)CraftMessageType;

        public virtual CraftMessageType CraftMessageType
        {
            get { throw new NotImplementedException(); }
        }

        public string PlayerName { get; set; }
    }
}