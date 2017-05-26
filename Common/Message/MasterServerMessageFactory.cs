using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.MasterServer;

namespace LunaCommon.Message
{
    /// <summary>
    ///     Class for deserialization of the master server messages that you've received
    /// </summary>
    public class MasterServerMessageFactory : FactoryBase
    {
        protected override Type HandledMessageTypes { get; } = typeof(ServerMessageType);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="compress">Compress the messages or not</param>
        public MasterServerMessageFactory(bool compress) : base(compress)
        {
            MessageDictionary.Add((uint)MasterServerMessageType.Main, new MainMstSrvMsg());
        }
    }
}