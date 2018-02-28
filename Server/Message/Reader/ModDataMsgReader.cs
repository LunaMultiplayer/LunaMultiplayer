using LunaCommon.Message.Data;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Message.Reader.Base;
using Server.Plugin;
using Server.Server;

namespace Server.Message.Reader
{
    public class ModDataMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (ModMsgData)message.Data;
            if (data.Relay)
                MessageQueuer.RelayMessage<ModSrvMsg>(client, data);
            LmpModInterface.OnModMessageReceived(client, data.ModName, data.Data, data.NumBytes);
        }
    }
}