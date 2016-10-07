using LunaCommon.Message.Interface;
using LunaServer.Client;

namespace LunaServer.Message.Reader.Base
{
    public abstract class ReaderBase
    {
        public abstract void HandleMessage(ClientStructure client, IMessageData messageData);
    }
}