using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class WarpSrvMsg : SrvMsgBase<WarpBaseMsgData>
    {
        /// <inheritdoc />
        internal WarpSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(WarpSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)WarpMessageType.SubspacesReply] = typeof(WarpSubspacesReplyMsgData),
            [(ushort)WarpMessageType.NewSubspace] = typeof(WarpNewSubspaceMsgData),
            [(ushort)WarpMessageType.ChangeSubspace] = typeof(WarpChangeSubspaceMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Warp;
        protected override int DefaultChannel => 13;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}