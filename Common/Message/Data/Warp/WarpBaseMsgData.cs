using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)WarpMessageType;

        public virtual WarpMessageType WarpMessageType => throw new NotImplementedException();
    }
}