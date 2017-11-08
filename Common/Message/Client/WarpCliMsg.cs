using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class WarpCliMsg : CliMsgBase<WarpBaseMsgData>
    {
        /// <inheritdoc />
        internal WarpCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)WarpMessageType.SubspacesRequest] = MessageStore.GetMessageData<WarpSubspacesRequestMsgData>(true),
            [(ushort)WarpMessageType.NewSubspace] = MessageStore.GetMessageData<WarpNewSubspaceMsgData>(true),
            [(ushort)WarpMessageType.ChangeSubspace] = MessageStore.GetMessageData<WarpChangeSubspaceMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.Warp;
        protected override int DefaultChannel => 13;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}