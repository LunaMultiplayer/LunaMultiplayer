using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class PlayerStatusCliMsg : CliMsgBase<PlayerStatusBaseMsgData>
    {
        /// <inheritdoc />
        internal PlayerStatusCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)PlayerStatusMessageType.Request] = MessageStore.GetMessageData<PlayerStatusRequestMsgData>(true),
            [(ushort)PlayerStatusMessageType.Set] = MessageStore.GetMessageData<PlayerStatusSetMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.PlayerStatus;
        protected override int DefaultChannel => 4;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}