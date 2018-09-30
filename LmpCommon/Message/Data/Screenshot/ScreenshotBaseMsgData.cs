using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Screenshot
{
    public abstract class ScreenshotBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal ScreenshotBaseMsgData() { }
        public override bool CompressCondition => false;
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