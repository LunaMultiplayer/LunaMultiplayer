using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Message.Reader.Base;
using Server.Server;

namespace Server.Message.Reader
{
    public class FacilityMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            //We don't do anything on the server side with this messages so just relay them.
            MessageQueuer.RelayMessage<FacilitySrvMsg>(client, messageData);
        }
    }
}