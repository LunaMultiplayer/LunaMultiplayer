using LmpCommon.Message.Interface;
using Server.Client;

namespace Server.Message.Base
{
    public abstract class ReaderBase
    {
        public abstract void HandleMessage(ClientStructure client, IClientMessageBase message);
    }
}
