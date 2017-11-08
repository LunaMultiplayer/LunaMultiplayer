using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class SetingsSrvMsg : SrvMsgBase<SettingsBaseMsgData>
    {
        /// <inheritdoc />
        internal SetingsSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)SettingsMessageType.Reply] = MessageStore.GetMessageData<SettingsReplyMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.Settings;
        protected override int DefaultChannel => 2;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}