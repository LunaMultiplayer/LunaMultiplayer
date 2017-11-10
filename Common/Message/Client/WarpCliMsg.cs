using System;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class WarpCliMsg : CliMsgBase<WarpBaseMsgData>
    {
        /// <inheritdoc />
        internal WarpCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)WarpMessageType.SubspacesRequest] = typeof(WarpSubspacesRequestMsgData),
            [(ushort)WarpMessageType.NewSubspace] = typeof(WarpNewSubspaceMsgData),
            [(ushort)WarpMessageType.ChangeSubspace] = typeof(WarpChangeSubspaceMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Warp;
        protected override int DefaultChannel => 13;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}