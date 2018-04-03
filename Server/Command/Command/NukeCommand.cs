using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using LunaCommon.Xml;
using Server.Command.Command.Base;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings;
using Server.System;
using System;
using System.Xml;

namespace Server.Command.Command
{
    public class NukeCommand : SimpleCommand
    {
        private static long _lastNukeTime;

        public static void CheckTimer()
        {
            //0 or less is disabled.
            if (GeneralSettings.SettingsStore.AutoNuke > 0 &&
                 ServerContext.ServerClock.ElapsedMilliseconds - _lastNukeTime >
                 TimeSpan.FromMinutes(GeneralSettings.SettingsStore.AutoNuke).TotalMilliseconds)
            {
                _lastNukeTime = ServerContext.ServerClock.ElapsedMilliseconds;
                RunNuke();
            }
        }

        public override bool Execute(string commandArgs)
        {
            RunNuke();
            return true;
        }

        private static void RunNuke()
        {
            var removalCount = 0;

            var vesselList = VesselStoreSystem.CurrentVesselsInXmlFormat.ToArray();
            foreach (var vesselKeyVal in vesselList)
            {
                if (IsVesselLandedAtKsc(vesselKeyVal.Value))
                {
                    LunaLog.Normal($"Removing vessel: {vesselKeyVal.Key} from KSC");

                    VesselStoreSystem.RemoveVessel(vesselKeyVal.Key);

                    //Send a vessel remove message
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
                    msgData.VesselId = vesselKeyVal.Key;

                    MessageQueuer.SendToAllClients<VesselSrvMsg>(msgData);

                    removalCount++;
                }
            }

            if (removalCount > 0)
                LunaLog.Normal($"Nuked {removalCount} vessels around the KSC");
        }
        
        private static bool IsVesselLandedAtKsc(string vesselData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var landed = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='landed']");
            if (landed?.InnerText == "True")
            {
                var landedAtNode = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='landedAt']");
                if (landedAtNode != null)
                    return landedAtNode.InnerText.ToLower().Contains("ksc") || landedAtNode.InnerText.ToLower().Contains("runway") || landedAtNode.InnerText.ToLower().Contains("launchpad");
            }
            
            return false;
        }
    }
}
