using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Screenshot;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class ScreenshotSrvMsg : SrvMsgBase<ScreenshotBaseMsgData>
    {
        /// <inheritdoc />
        internal ScreenshotSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ScreenshotSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)ScreenshotMessageType.ScreenshotData] = typeof(ScreenshotDataMsgData),
        };

        public override ServerMessageType MessageType => ServerMessageType.Screenshot;
        protected override int DefaultChannel => 20;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableUnordered;
    }
}