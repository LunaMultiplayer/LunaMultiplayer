using System;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)MasterServerMessageSubType;

        public virtual MasterServerMessageSubType MasterServerMessageSubType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
