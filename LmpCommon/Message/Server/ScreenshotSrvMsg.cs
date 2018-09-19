using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Screenshot;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
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
            [(ushort)ScreenshotMessageType.FoldersReply] = typeof(ScreenshotFoldersReplyMsgData),
            [(ushort)ScreenshotMessageType.ListReply] = typeof(ScreenshotListReplyMsgData),
            [(ushort)ScreenshotMessageType.ScreenshotData] = typeof(ScreenshotDataMsgData),
            [(ushort)ScreenshotMessageType.Notification] = typeof(ScreenshotNotificationMsgData),
        };

        public override ServerMessageType MessageType => ServerMessageType.Screenshot;
        protected override int DefaultChannel => 20;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
