using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Server;
using LmpCommon.Xml;
using Server.Command.Command.Base;
using Server.Command.Common;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.System;
using System;
using System.Linq;
using System.Xml;

namespace Server.Command.Command
{
    public class ClearVesselsCommand : SimpleCommand
    {
        //Executes the ClearVesselsCommand
        public override bool Execute(string commandArgs)
        {
            //Get param array from command
            CommandSystemHelperMethods.SplitCommandParamArray(commandArgs, out var parameters);

            //Declare variables
            var paramCount = 5;

            var msgUsage = Environment.NewLine
                            + Environment.NewLine + "Usage:"
                            + Environment.NewLine + "/clearvessels vesselType vesselSituation vesselSplashed vesselName orbName";

            var msgDescription = Environment.NewLine
                               + Environment.NewLine + "Description:"
                               + Environment.NewLine + "You can use * on a search param to accept all possible values."
                               + Environment.NewLine + "Using * for all params like in the following example"
                               + Environment.NewLine + "will clear every vessel from universe."
                               + Environment.NewLine + "/clearvessels * * * * *"
                               + Environment.NewLine + "vesselType can be ship, plane, debris, spaceobject etc."
                               + Environment.NewLine + "vesselSituation can be orbiting, flying, landed."
                               + Environment.NewLine + "vesselSplashed can be true or false."
                               + Environment.NewLine + "vesselName is the given vessel name."
                               + Environment.NewLine + "Checks if the name contains the given vesselName."
                               + Environment.NewLine + "orbName is the name of the orb the vessel is close to."
                               + Environment.NewLine + "For example kerbin or duna."
                               + Environment.NewLine + "Checks if the orb name contains the given orbName."
                               + Environment.NewLine + "The following command clears all vessels from"
                               + Environment.NewLine + "kerbin orbit that are planes and have"
                               + Environment.NewLine + "name containing the word aeris."
                               + Environment.NewLine + "/clearvessels plane orbiting * aeris kerbin";

            var vesselType = "";
            var vesselSituation = "";
            var vesselSplashed = "";
            var vesselName = "";
            var orbName = "";

            try
            {
                //Check number of parameters and if they are null or empty
                if (parameters != null && !string.IsNullOrEmpty(parameters[0]) && parameters.Length == paramCount)
                {
                    //Get params
                    if (!string.IsNullOrEmpty(parameters[0])) vesselType = parameters[0];
                    if (!string.IsNullOrEmpty(parameters[1])) vesselSituation = parameters[1];
                    if (!string.IsNullOrEmpty(parameters[2])) vesselSplashed = parameters[2];
                    if (!string.IsNullOrEmpty(parameters[3])) vesselName = parameters[3];
                    if (!string.IsNullOrEmpty(parameters[4])) orbName = parameters[4];

                    //Check params
                    vesselType = string.IsNullOrEmpty(vesselType) ? "" : vesselType;
                    vesselSituation = string.IsNullOrEmpty(vesselSituation) ? "" : vesselSituation;
                    vesselSplashed = string.IsNullOrEmpty(vesselSplashed) ? "" : vesselSplashed;
                    vesselName = string.IsNullOrEmpty(vesselName) ? "" : vesselName;
                    orbName = string.IsNullOrEmpty(orbName) ? "" : orbName;

                    //Remove vessels
                    RunRemove(vesselType, vesselSituation, vesselSplashed, vesselName, orbName);
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
        private static void RunRemove(string vesselType, string vesselSituation, string vesselSplashed, string vesselName, string orbName)
        {
            //Declare variables
            var removalCount = 0;
            var vesselList = VesselStoreSystem.CurrentVesselsInXmlFormat.ToArray();

            //Cycle vesselList
            foreach (var vesselKeyVal in vesselList)
            {
                //Check if vessel can be found
                if (IsVesselFound(vesselKeyVal.Value, vesselType, vesselSituation, vesselSplashed, vesselName, orbName))
                {
                    //Send a vessel remove server console message
                    LunaLog.Normal($"Removing vessel: {vesselKeyVal.Key}");

                    //Remove vessel from universe
                    VesselStoreSystem.RemoveVessel(vesselKeyVal.Key);

                    //Send a vessel remove message to all clients
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
                    msgData.VesselId = vesselKeyVal.Key;
                    MessageQueuer.SendToAllClients<VesselSrvMsg>(msgData);

                    //Increase counter
                    removalCount++;
                }
            }
            //Send message remove info
            if (removalCount > 0)
                LunaLog.Normal($"Removed {removalCount} vessel(s) ...");
            else
                LunaLog.Normal($"Removed nothing ...");
        }

        //Searches for matching vessels, returns true if all search params matched and if the vessel was found
        private static bool IsVesselFound(string vesselData, string vesselType, string vesselSituation, string vesselSplashed, string vesselName, string orbName)
        {
            //Check if params are empty
            if (vesselType != "" && vesselSituation != "" && vesselSplashed != "" && vesselName != "" && orbName != "")
            {
                //Read from XML-Document
                var document = new XmlDocument();
                document.LoadXml(vesselData);

                //Declare Search-Match-Booleans
                var isVesselType = false;
                var isVesselSituation = false;
                var isVesselSplashed = false;
                var isVesselName = false;
                var isOrbName = false;

                //Get XML-Nodes
                var nodeVesselType = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='type']");
                var nodeVesselSituation = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='sit']");
                var nodeVesselSplashed = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='splashed']");
                var nodeVesselName = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='name']");
                var nodeOrbName = document.SelectSingleNode("//Log/Parameter[@name='0']");
                //Filter the orb name
                var nodeOrbNameParams = nodeOrbName.InnerText.Split(',').ToList<string>();
                nodeOrbNameParams.Reverse();
                var stringOrbName = nodeOrbNameParams[0];

                //Check if vessel matches search param
                if (nodeVesselType == null) isVesselType = false;
                else if (vesselType == "*" || nodeVesselType.InnerText.ToLower() == vesselType.ToLower()) isVesselType = true;
                if (nodeVesselSituation == null) isVesselSituation = false;
                else if(vesselSituation == "*" || nodeVesselSituation.InnerText.ToLower() == vesselSituation.ToLower()) isVesselSituation = true;
                if (nodeVesselSplashed == null) isVesselSplashed = false;
                else if (vesselSplashed == "*" || nodeVesselSplashed.InnerText.ToLower() == vesselSplashed.ToLower()) isVesselSplashed = true;
                if (nodeVesselName == null) isVesselName = false;
                else if (vesselName == "*" || nodeVesselName.InnerText.ToLower().Contains(vesselName.ToLower())) isVesselName = true;
                if (nodeOrbName == null) isOrbName = false;
                else if (orbName == "*" || stringOrbName.ToLower().Contains(orbName.ToLower())) isOrbName = true;

                //Check if all search params matched
                if (isVesselType && isVesselSituation && isVesselSplashed && isVesselName && isOrbName)
                    return true;
                //If one ore more bools are false return false
                else
                    return false;
            }
            //If param is empty return false
            else
            {
                return false;
            }
        }
    }
}
