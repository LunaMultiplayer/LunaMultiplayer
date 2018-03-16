using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Screenshot;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
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
            [(ushort)ScreenshotMessageType.ListRequest] = typeof(ScreenshotListRequestMsgData),
            [(ushort)ScreenshotMessageType.DownloadRequest] = typeof(ScreenshotDownloadRequestMsgData),
            [(ushort)ScreenshotMessageType.Upload] = typeof(ScreenshotUploadMsgData),
        };

        public override ClientMessageType MessageType => ClientMessageType.Screenshot;
        protected override int DefaultChannel => 19;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableUnordered;
    }
}