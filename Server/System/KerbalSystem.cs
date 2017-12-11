using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Properties;
using Server.Server;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.System
{
    public class KerbalSystem
    {
        public static readonly string KerbalsPath = Path.Combine(ServerContext.UniverseDirectory, "Kerbals");
        
        public static void GenerateDefaultKerbals()
        {
            LunaLog.Normal("Creating default kerbals...");
            FileHandler.WriteToFile(Path.Combine(KerbalsPath, "Jebediah Kerman.txt"), Resources.Jebediah_Kerman);
            FileHandler.WriteToFile(Path.Combine(KerbalsPath, "Bill Kerman.txt"), Resources.Bill_Kerman);
            FileHandler.WriteToFile(Path.Combine(KerbalsPath, "Bob Kerman.txt"), Resources.Bob_Kerman);
            FileHandler.WriteToFile(Path.Combine(KerbalsPath, "Valentina Kerman.txt"), Resources.Valentina_Kerman);
        }

        public static void HandleKerbalProto(ClientStructure client, KerbalProtoMsgData data)
        {
            LunaLog.Debug($"Saving kerbal {data.KerbalName} from {client.PlayerName}");

            var path = Path.Combine(KerbalsPath, $"{data.KerbalName}.txt");
            FileHandler.WriteToFile(path, data.KerbalData);

            MessageQueuer.RelayMessage<KerbalSrvMsg>(client, data);
        }

        public static void HandleKerbalsRequest(ClientStructure client)
        {
            var kerbalFiles = FileHandler.GetFilesInPath(KerbalsPath);
            var kerbalsData = kerbalFiles.Select(k => new KeyValuePair<string, byte[]>(Path.GetFileNameWithoutExtension(k),
                FileHandler.ReadFile(k)));

            LunaLog.Debug($"Sending {client.PlayerName} {kerbalFiles.Length} kerbals...");

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<KerbalReplyMsgData>();
            msgData.KerbalsData = kerbalsData.ToArray();

            MessageQueuer.SendToClient<KerbalSrvMsg>(client, msgData);
        }

        public static void HandleKerbalRemove(ClientStructure client, KerbalRemoveMsgData message)
        {
            var kerbalToRemove = message.KerbalName;

            LunaLog.Debug($"Removing kerbal {kerbalToRemove} from {client.PlayerName}");
            FileHandler.FileDelete(Path.Combine(KerbalsPath, $"{kerbalToRemove}.txt"));

            MessageQueuer.RelayMessage<KerbalSrvMsg>(client, message);
        }
    }
}
