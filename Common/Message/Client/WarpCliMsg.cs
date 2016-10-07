using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class WarpCliMsg : CliMsgBase<WarpBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)WarpMessageType.SUBSPACES_REQUEST] = new WarpSubspacesRequestMsgData(),
            [(ushort)WarpMessageType.NEW_SUBSPACE] = new WarpNewSubspaceMsgData(),
            [(ushort)WarpMessageType.CHANGE_SUBSPACE] = new WarpChangeSubspaceMsgData(),
        };

        public override ClientMessageType MessageType => ClientMessageType.WARP;
        protected override int DefaultChannel => 13;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}