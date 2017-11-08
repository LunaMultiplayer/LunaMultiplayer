using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal KerbalBaseMsgData() { }
        public override ushort SubType => (ushort)(int)KerbalMessageType;

        public virtual KerbalMessageType KerbalMessageType => throw new NotImplementedException();
    }
}
