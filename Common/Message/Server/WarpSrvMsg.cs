using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class WarpSrvMsg : SrvMsgBase<WarpBaseMsgData>
    {
        /// <inheritdoc />
        internal WarpSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)WarpMessageType.SubspacesReply] = MessageStore.GetMessageData<WarpSubspacesReplyMsgData>(true),
            [(ushort)WarpMessageType.NewSubspace] = MessageStore.GetMessageData<WarpNewSubspaceMsgData>(true),
            [(ushort)WarpMessageType.ChangeSubspace] = MessageStore.GetMessageData<WarpChangeSubspaceMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.Warp;
        protected override int DefaultChannel => 13;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}