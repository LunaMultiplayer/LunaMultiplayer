using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.PlayerConnection;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class PlayerConnectionSrvMsg : SrvMsgBase<PlayerConnectionBaseMsgData>
    {
        /// <inheritdoc />
        internal PlayerConnectionSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)PlayerConnectionMessageType.Join] = MessageStore.GetMessageData<PlayerConnectionJoinMsgData>(true),
            [(ushort)PlayerConnectionMessageType.Leave] = MessageStore.GetMessageData<PlayerConnectionLeaveMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.PlayerConnection;
        protected override int DefaultChannel => 17;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}