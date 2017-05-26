using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Client
{
    public class WarpCliMsg : CliMsgBase<WarpBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)WarpMessageType.SubspacesRequest] = new WarpSubspacesRequestMsgData(),
            [(ushort)WarpMessageType.NewSubspace] = new WarpNewSubspaceMsgData(),
            [(ushort)WarpMessageType.ChangeSubspace] = new WarpChangeSubspaceMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.Warp;
        protected override int DefaultChannel => 13;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}