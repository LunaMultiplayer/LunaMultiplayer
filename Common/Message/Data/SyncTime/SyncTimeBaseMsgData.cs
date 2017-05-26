using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.SyncTime
{
    public class SyncTimeBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)SyncTimeMessageType;

        public virtual SyncTimeMessageType SyncTimeMessageType => throw new NotImplementedException();
    }
}
