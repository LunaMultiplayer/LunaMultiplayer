using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using LunaServer.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaServer.Message.Reader
{
    public class KerbalMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as KerbalBaseMsgData;
            switch (message?.KerbalMessageType)
            {
                case KerbalMessageType.Request:
                    var kerbalFiles = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Kerbals"));
                    var kerbalsData = kerbalFiles.Select(k => new KeyValuePair<string, byte[]>(Path.GetFileNameWithoutExtension(k),
                        FileHandler.ReadFile(k)));

                    LunaLog.Debug($"Sending {client.PlayerName} {kerbalFiles.Length} kerbals...");

                    var newMessageData = new KerbalReplyMsgData
                    {
                        KerbalsData = kerbalsData.ToArray()
                    };

                    MessageQueuer.SendToClient<KerbalSrvMsg>(client, newMessageData);
                    break;
                case KerbalMessageType.Proto:
                    var data = (KerbalProtoMsgData)message;

                    LunaLog.Debug($"Saving kerbal {data.KerbalName} from {client.PlayerName}");

                    var path = Path.Combine(ServerContext.UniverseDirectory, "Kerbals", $"{data.KerbalName}.txt");
                    FileHandler.WriteToFile(path, data.KerbalData);

                    MessageQueuer.RelayMessage<KerbalSrvMsg>(client, data);
                    break;
                default:
                    throw new NotImplementedException("Kerbal type not implemented");
            }
        }
    }
}