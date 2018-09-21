using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Server;
using Server.Command.Command.Base;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings.Structures;
using Server.System;
using System;

namespace Server.Command.Command
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

        public override bool Execute(string commandArgs)
        {
            RunDekessler();
            return true;
        }

        private static void RunDekessler()
        {
            var removalCount = 0;

            var vesselList = VesselStoreSystem.CurrentVessels.ToArray();
            foreach (var vesselKeyVal in vesselList)
            {
                if (vesselKeyVal.Value.Fields.GetSingle("type").Value == "debris")
                {
                    LunaLog.Normal($"Removing debris vessel: {vesselKeyVal.Key}");

                    VesselStoreSystem.RemoveVessel(vesselKeyVal.Key);

                    //Send a vessel remove message
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
                    msgData.VesselId = vesselKeyVal.Key;

                    MessageQueuer.SendToAllClients<VesselSrvMsg>(msgData);

                    removalCount++;
                }
            }

            if (removalCount > 0)
                LunaLog.Normal($"Removed {removalCount} debris");
        }
    }
}
