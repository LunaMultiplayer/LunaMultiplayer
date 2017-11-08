using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal CraftLibraryBaseMsgData() { }
        public override ushort SubType => (ushort)(int)CraftMessageType;

        public virtual CraftMessageType CraftMessageType => throw new NotImplementedException();

        public string PlayerName { get; set; }
    }
}