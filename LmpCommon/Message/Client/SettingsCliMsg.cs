using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Settings;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class SettingsCliMsg : CliMsgBase<SettingsBaseMsgData>
    {
        /// <inheritdoc />
        internal SettingsCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(SettingsCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)SettingsMessageType.Request] = typeof(SettingsRequestMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Settings;
        protected override int DefaultChannel => 2;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
