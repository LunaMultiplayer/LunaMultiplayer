using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class SetingsSrvMsg : SrvMsgBase<SettingsBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)SettingsMessageType.REPLY] = new SettingsReplyMsgData(),
        };

        public override ServerMessageType MessageType => ServerMessageType.SETTINGS;
        protected override int DefaultChannel => 2;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}