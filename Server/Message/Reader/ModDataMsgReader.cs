using LunaCommon.Message.Data;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.Plugin;
using LunaServer.Server;

namespace LunaServer.Message.Reader
{
    public class ModDataMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var data = (ModMsgData) message;
            if (data.Relay)
                MessageQueuer.RelayMessage<ModSrvMsg>(client, data);
            LmpModInterface.OnModMessageReceived(client, data.ModName, data.Data);
        }
    }
}