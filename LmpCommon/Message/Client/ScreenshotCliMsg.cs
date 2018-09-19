using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Screenshot;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class ScreenshotCliMsg : CliMsgBase<ScreenshotBaseMsgData>
    {
        /// <inheritdoc />
        internal ScreenshotCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ScreenshotCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)ScreenshotMessageType.FoldersRequest] = typeof(ScreenshotFoldersRequestMsgData),
            [(ushort)ScreenshotMessageType.ListRequest] = typeof(ScreenshotListRequestMsgData),
            [(ushort)ScreenshotMessageType.DownloadRequest] = typeof(ScreenshotDownloadRequestMsgData),
            [(ushort)ScreenshotMessageType.ScreenshotData] = typeof(ScreenshotDataMsgData),
        };

        public override ClientMessageType MessageType => ClientMessageType.Screenshot;
        protected override int DefaultChannel => 19;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}