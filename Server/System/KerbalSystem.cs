using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Properties;
using LunaServer.Server;
using LunaServer.Settings;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaServer.System
{
    public class KerbalSystem
    {
        public static readonly string KerbalsPath = Path.Combine(ServerContext.UniverseDirectory, "Kerbals");
        
        public static void GenerateDefaultKerbals()
        {
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
            if (!GeneralSettings.SettingsStore.RelayKerbalRemove) return;

            var kerbalToRemove = message.KerbalName;

            LunaLog.Debug($"Removing kerbal {kerbalToRemove} from {client.PlayerName}");
            FileHandler.FileDelete(Path.Combine(KerbalsPath, $"{kerbalToRemove}.txt"));

            MessageQueuer.RelayMessage<KerbalSrvMsg>(client, message);
        }
    }
}
