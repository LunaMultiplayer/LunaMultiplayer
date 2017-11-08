using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.MasterServer;
using System;

namespace LunaCommon.Message
{
    /// <summary>
    ///     Class for deserialization of the master server messages that you've received
    /// </summary>
    public class MasterServerMessageFactory : FactoryBase
    {
        protected override Type HandledMessageTypes { get; } = typeof(ServerMessageType);

        /// <inheritdoc />
        public MasterServerMessageFactory(bool compress) : base(compress)
        {
            MessageDictionary.Add((uint)MasterServerMessageType.Main, typeof(MainMstSrvMsg));
        }
    }
}