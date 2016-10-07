using System;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)WarpMessageType;

        public virtual WarpMessageType WarpMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}