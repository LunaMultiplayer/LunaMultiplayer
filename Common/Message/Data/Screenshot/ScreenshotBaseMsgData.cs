using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Screenshot
{
    public abstract class ScreenshotBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal ScreenshotBaseMsgData() { }
        public override ushort SubType => (ushort)(int)ScreenshotMessageType;
        public virtual ScreenshotMessageType ScreenshotMessageType => throw new NotImplementedException();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize()
        {
            return 0;
        }
    }
}