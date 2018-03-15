using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending technology unlocks between clients.
    /// </summary>
    public class ShareProgressTechnologyMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressTechnologyMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.TechnologyUpdate;

        public string TechID;

        public override string ClassName { get; } = nameof(ShareProgressTechnologyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            
            lidgrenMsg.Write(TechID);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            
            TechID = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + TechID.GetByteCount();
        }
    }
}
