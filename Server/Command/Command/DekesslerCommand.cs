using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using LunaServer.Command.Command.Base;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.Settings;
using LunaServer.System;
using System;
using System.IO;
using System.Linq;

namespace LunaServer.Command.Command
{
    public class DekesslerCommand : SimpleCommand
    {
        private static long _lastDekesslerTime;

        public static void CheckTimer()
        {
            //0 or less is disabled.
            if (GeneralSettings.SettingsStore.AutoDekessler > 0 &&
                ServerContext.ServerClock.ElapsedMilliseconds - _lastDekesslerTime >
                TimeSpan.FromMinutes(GeneralSettings.SettingsStore.AutoDekessler).TotalMilliseconds)
            {
                _lastDekesslerTime = ServerContext.ServerClock.ElapsedMilliseconds;
                RunDekessler();
            }
        }

        public override void Execute(string commandArgs)
        {
            RunDekessler();
        }

        private static void RunDekessler()
        {
            var vesselList = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels"));
            var removalCount = 0;
            foreach (var vesselFilePath in vesselList)
            {
                var vesselId = Path.GetFileNameWithoutExtension(vesselFilePath);
                var vesselIsDebris = FileHandler.ReadFileLines(vesselFilePath).Select(l=> l.ToLower())
                    .Any(l => l.Contains("type = ") && l.Contains("debris"));

                if (vesselId != null && vesselIsDebris)
                {
                    LunaLog.Normal($"Removing debris vessel: {vesselId}");

                    //Delete it from the universe
                    Universe.RemoveFromUniverse(vesselFilePath);

                    //Send a vessel remove message
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
                    msgData.VesselId = Guid.Parse(vesselId);

                    MessageQueuer.SendToAllClients<VesselSrvMsg>(msgData);

                    removalCount++;
                }
            }

            if (removalCount > 0)
                LunaLog.Normal($"Removed {removalCount} debris");
        }
    }
}