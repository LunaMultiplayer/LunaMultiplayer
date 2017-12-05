using LMP.Server.Client;
using LunaCommon.Message.Interface;

namespace LMP.Server.Message.Reader.Base
{
    public abstract class ReaderBase
    {
        public abstract void HandleMessage(ClientStructure client, IMessageData messageData);
    }
}