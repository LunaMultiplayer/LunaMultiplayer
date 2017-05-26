using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Server
{
    public class WarpSrvMsg : SrvMsgBase<WarpBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)WarpMessageType.SubspacesReply] = new WarpSubspacesReplyMsgData(),
            [(ushort)WarpMessageType.NewSubspace] = new WarpNewSubspaceMsgData(),
            [(ushort)WarpMessageType.ChangeSubspace] = new WarpChangeSubspaceMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.Warp;
        protected override int DefaultChannel => 13;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}