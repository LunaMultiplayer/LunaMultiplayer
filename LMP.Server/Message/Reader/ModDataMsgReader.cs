using LMP.Server.Client;
using LMP.Server.Message.Reader.Base;
using LMP.Server.Plugin;
using LMP.Server.Server;
using LunaCommon.Message.Data;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;

namespace LMP.Server.Message.Reader
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