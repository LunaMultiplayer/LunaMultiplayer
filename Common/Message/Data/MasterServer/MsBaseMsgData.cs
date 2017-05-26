using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsBaseMsgData : MessageData
    {
        /// <summary>
        /// Master server messages does not have versions!
        /// </summary>
        public override string Version => "0.0.0.0";

        public override ushort SubType => (ushort)(int)MasterServerMessageSubType;

        public virtual MasterServerMessageSubType MasterServerMessageSubType => throw new NotImplementedException();
    }
}
