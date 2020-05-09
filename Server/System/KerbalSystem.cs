using LmpCommon.Message.Data.Kerbal;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Properties;
using Server.Server;
using System.IO;
using System.Linq;
using Server.Settings.Structures;

namespace Server.System
{
    public class KerbalSystem
    {
        public static readonly string KerbalsPath = Path.Combine(ServerContext.UniverseDirectory, "Kerbals");

        public static void GenerateDefaultKerbals()
        {
            FileHandler.CreateFile(Path.Combine(KerbalsPath, "Jebediah Kerman.txt"), Resources.Jebediah_Kerman);
            FileHandler.CreateFile(Path.Combine(KerbalsPath, "Bill Kerman.txt"), Resources.Bill_Kerman);
            FileHandler.CreateFile(Path.Combine(KerbalsPath, "Bob Kerman.txt"), Resources.Bob_Kerman);
            FileHandler.CreateFile(Path.Combine(KerbalsPath, "Valentina Kerman.txt"), Resources.Valentina_Kerman);
        }

        public static void HandleKerbalProto(ClientStructure client, KerbalProtoMsgData data)
        {
            LunaLog.Debug($"Saving kerbal {data.Kerbal.KerbalName} from {client.PlayerName}");

            var kerbalsPath = GetPlayerKerbalsPath(client);

            var path = Path.Combine(kerbalsPath, $"{data.Kerbal.KerbalName}.txt");
            FileHandler.WriteToFile(path, data.Kerbal.KerbalData, data.Kerbal.NumBytes);

            MessageQueuer.RelayMessage<KerbalSrvMsg>(client, data);
        }

        public static void HandleKerbalsRequest(ClientStructure client)
        {
            var kerbalsPath = GetPlayerKerbalsPath(client);

            var kerbalFiles = FileHandler.GetFilesInPath(kerbalsPath);
            var kerbalsData = kerbalFiles.Select(k =>
            {
                var kerbalData = FileHandler.ReadFile(k);
                return new KerbalInfo
                {
                    KerbalData = kerbalData,
                    NumBytes = kerbalData.Length,
                    KerbalName = Path.GetFileNameWithoutExtension(k)
                };
            });
            LunaLog.Debug($"Sending {client.PlayerName} {kerbalFiles.Length} kerbals...");

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<KerbalReplyMsgData>();
            msgData.Kerbals = kerbalsData.ToArray();
            msgData.KerbalsCount = msgData.Kerbals.Length;

            MessageQueuer.SendToClient<KerbalSrvMsg>(client, msgData);
        }

        public static void HandleKerbalRemove(ClientStructure client, KerbalRemoveMsgData message)
        {
            var kerbalsPath = GetPlayerKerbalsPath(client);

            var kerbalToRemove = message.KerbalName;

            LunaLog.Debug($"Removing kerbal {kerbalToRemove} from {client.PlayerName}");
            FileHandler.FileDelete(Path.Combine(kerbalsPath, $"{kerbalToRemove}.txt"));

            MessageQueuer.RelayMessage<KerbalSrvMsg>(client, message);
        }

        private static string GetPlayerKerbalsPath(ClientStructure client)
        {
            if (!GameplaySettings.SettingsStore.AllowPerPlayerKerbals)
                return KerbalsPath;

            var playerDir = Path.Combine(ServerContext.PlayerDataPath, client.UniqueIdentifier);
            if (!FileHandler.FolderExists(playerDir))
            {
                FileHandler.FolderCreate(playerDir);

                FileHandler.CreateFile(Path.Combine(playerDir, "Jebediah Kerman.txt"), Resources.Jebediah_Kerman);
                FileHandler.CreateFile(Path.Combine(playerDir, "Bill Kerman.txt"), Resources.Bill_Kerman);
                FileHandler.CreateFile(Path.Combine(playerDir, "Bob Kerman.txt"), Resources.Bob_Kerman);
                FileHandler.CreateFile(Path.Combine(playerDir, "Valentina Kerman.txt"), Resources.Valentina_Kerman);
            }
            return playerDir;
        }
    }
}
