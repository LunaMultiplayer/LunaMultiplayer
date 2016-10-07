using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)KerbalMessageType;

        public virtual KerbalMessageType KerbalMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
