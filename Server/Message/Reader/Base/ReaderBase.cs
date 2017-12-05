using LunaCommon.Message.Interface;
using Server.Client;

namespace Server.Message.Reader.Base
{
    public abstract class ReaderBase
    {
        public abstract void HandleMessage(ClientStructure client, IMessageData messageData);
    }
}