using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class SetingsSrvMsg : SrvMsgBase<SettingsBaseMsgData>
    {
        /// <inheritdoc />
        internal SetingsSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(SetingsSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)SettingsMessageType.Reply] = typeof(SettingsReplyMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Settings;
        protected override int DefaultChannel => 2;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}