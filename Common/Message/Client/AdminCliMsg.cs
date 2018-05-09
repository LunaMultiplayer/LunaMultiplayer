using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class AdminCliMsg : CliMsgBase<AdminBaseMsgData>
    {
        /// <inheritdoc />
        internal AdminCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(AdminCliMsg);

        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)AdminMessageType.Ban] = typeof(AdminBanMsgData),
            [(ushort)AdminMessageType.Kick] = typeof(AdminKickMsgData),
            [(ushort)AdminMessageType.Dekessler] = typeof(AdminDekesslerMsgData),
            [(ushort)AdminMessageType.Nuke] = typeof(AdminNukeMsgData),
            [(ushort)AdminMessageType.RestartServer] = typeof(AdminRestartServerMsgData),
        };

        public override ClientMessageType MessageType => ClientMessageType.Admin;
        protected override int DefaultChannel => 16;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
