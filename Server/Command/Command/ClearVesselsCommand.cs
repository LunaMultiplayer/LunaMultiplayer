using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Server;
using Server.Command.Command.Base;
using Server.Command.Common;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Structures;
using Server.System;
using System;
using System.Linq;

namespace Server.Command.Command
{
    public class ClearVesselsCommand : SimpleCommand
    {
        //Executes the ClearVesselsCommand
        public override bool Execute(string commandArgs)
        {
            CommandSystemHelperMethods.SplitCommandParamArray(commandArgs, out var parameters);
            
            var msgUsage = $"{Environment.NewLine}{Environment.NewLine}" +
                           $"Usage:{Environment.NewLine}" +
                           "/clearvessels vesselType vesselSituation vesselSplashed vesselName";

            var msgDescription = $"{Environment.NewLine}{Environment.NewLine}" +
                                 $"Description:{Environment.NewLine}" +
                                 $"You can use * on a search param to accept all possible values.{Environment.NewLine}" +
                                 $"Using * for all params like in the following example{Environment.NewLine}" +
                                 $"will clear every vessel from universe.{Environment.NewLine}" +
                                 $"/clearvessels * * * * *{Environment.NewLine}" +
                                 $"vesselType can be ship, plane, debris, spaceobject etc.{Environment.NewLine}" +
                                 $"vesselSituation can be orbiting, flying, landed.{Environment.NewLine}" +
                                 $"vesselSplashed can be true or false.{Environment.NewLine}" +
                                 $"vesselName is the given vessel name.{Environment.NewLine}" +
                                 $"Example: /clearvessels plane orbiting * aeris{Environment.NewLine}" +
                                 $"Clears all vessels that are planes and have{Environment.NewLine}" +
                                 "name containing the word aeris.";

            try
            {
                if (parameters.Length == 4)
                {
                    var vesselType = string.IsNullOrEmpty(parameters[0]) ? "" : parameters[0];
                    var vesselSituation = string.IsNullOrEmpty(parameters[1]) ? "" : parameters[1];
                    var vesselSplashed = string.IsNullOrEmpty(parameters[2]) ? "" : parameters[2];
                    var vesselName = string.IsNullOrEmpty(parameters[3]) ? "" : parameters[3];

                    RunRemove(vesselType, vesselSituation, vesselSplashed, vesselName);
                }
                else
                {
                    LunaLog.Error($"{Environment.NewLine}Syntax error. Wrong number of parameters.{msgUsage}{msgDescription}");
                    return false;
                }
            }
            catch (Exception)
            {
                LunaLog.Error($"{Environment.NewLine}Syntax error.{msgUsage}{msgDescription}");
                return false;
            }

            return true;
        }

        //Removes all matching vessels
        private static void RunRemove(string vesselType, string vesselSituation, string vesselSplashed, string vesselName)
        {
            var removalCount = 0;
            var vesselList = VesselStoreSystem.CurrentVessels.ToArray();

            foreach (var vesselKeyVal in vesselList.Where(v=> IsVesselFound(v.Value, vesselType, vesselSituation, vesselSplashed, vesselName)))
            {
                LunaLog.Normal($"Removing vessel: {vesselKeyVal.Key}");

                VesselStoreSystem.RemoveVessel(vesselKeyVal.Key);

                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
                msgData.VesselId = vesselKeyVal.Key;
                MessageQueuer.SendToAllClients<VesselSrvMsg>(msgData);

                removalCount++;
            }

            LunaLog.Normal(removalCount > 0 ? $"Removed {removalCount} vessel(s) ..." : "Removed nothing ...");
        }

        /// <summary>
        /// Searches for matching vessels, returns true if all search params matched and if the vessel was found
        /// </summary>
        private static bool IsVesselFound(Vessel vessel, string vesselType, string vesselSituation, string vesselSplashed, string vesselName)
        {
            if (!string.IsNullOrEmpty(vesselType) && !string.IsNullOrEmpty(vesselSituation) && !string.IsNullOrEmpty(vesselSplashed) 
                && !string.IsNullOrEmpty(vesselName))
            {
                var currentVesselType = vessel.Fields.GetSingle("type").Value;
                var currentVesselSituation = vessel.Fields.GetSingle("sit").Value;
                var currentVesselSplashed = vessel.Fields.GetSingle("splashed").Value;
                var currentVesselName = vessel.Fields.GetSingle("name").Value;
                
                var isVesselType = vesselType == "*" || string.Equals(currentVesselType, vesselType, StringComparison.CurrentCultureIgnoreCase);
                var isVesselSituation = vesselSituation == "*" || string.Equals(currentVesselSituation, vesselSituation, StringComparison.CurrentCultureIgnoreCase);
                var isVesselSplashed = vesselSplashed == "*" || string.Equals(currentVesselSplashed, vesselSplashed, StringComparison.CurrentCultureIgnoreCase);
                var isVesselName = vesselName == "*" || currentVesselName.ToLower().Contains(vesselName.ToLower());

                return isVesselType && isVesselSituation && isVesselSplashed && isVesselName;
            }

            return false;
        }
    }
}
