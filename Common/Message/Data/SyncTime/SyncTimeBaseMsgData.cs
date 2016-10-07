using System;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.SyncTime
{
    public class SyncTimeBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)SyncTimeMessageType;

        public virtual SyncTimeMessageType SyncTimeMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
