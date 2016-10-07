using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LunaCommon.Message.Data;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using LunaServer.Command.Command.Base;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.Settings;
using LunaServer.System;

namespace LunaServer.Command.Command
{
    public class Dekessler : SimpleCommand
    {
        public static long LastDekesslerTime;

        public static void RunDekessler(string commandText)
        {
            var vesselList = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels"));
            var removalCount = 0;
            foreach (var vesselFilePath in vesselList)
            {
                var vesselIsDebris = FileHandler.ReadFileLines(vesselFilePath)
                    .Any(l => l.Contains("Type = ") && l.Contains("Debris"));

                if (vesselIsDebris)
                {
                    var vesselId = Path.GetFileNameWithoutExtension(vesselFilePath);
                    LunaLog.Normal("Removing vessel: " + vesselId);

                    //Delete it from the universe
                    Universe.RemoveFromUniverse(vesselFilePath);

                    //Send a vessel remove message
                    //Send it with a delete time of 0 so it shows up for all Players.
                    MessageQueuer.SendToAllClients<VesselSrvMsg>(new VesselRemoveMsgData
                    {
                        PlanetTime = 0,
                        VesselId = Guid.Parse(vesselId)
                    });

                    removalCount++;
                    LunaLog.Debug("Removed debris vessel " + vesselId);
                }
            }
            LunaLog.Normal("Removed " + removalCount + " debris");
        }

        public static void CheckTimer()
        {
            //0 or less is disabled.
            if (GeneralSettings.SettingsStore.AutoDekessler > 0)
                if ((ServerContext.ServerClock.ElapsedMilliseconds - LastDekesslerTime >
                     GeneralSettings.SettingsStore.AutoDekessler*60*1000) || (LastDekesslerTime == 0))
                {
                    LastDekesslerTime = ServerContext.ServerClock.ElapsedMilliseconds;
                    RunDekessler("");
                }
        }

        public override void Execute(string commandArgs)
        {
            RunDekessler("");
        }
    }
}