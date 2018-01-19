using LunaCommon.Message.Data.Vessel;
using Server.Context;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Server.System
{
    /// <summary>
    /// We try to avoid working with protovessels as much as possible as they can be huge files.
    /// This class patches the vessel file with the information messages we receive about a position and other vessel properties.
    /// This way we send the whole vessel definition only when there are parts that have changed 
    /// </summary>
    public class VesselFileUpdater
    {
        /// <summary>
        /// Update the vessel files max at a 2,5 seconds interval
        /// </summary>
        private const int FileUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        /// <summary>
        /// We received a position/update information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an update vesselproto
        /// </summary>
        public static void RewriteVesselFile(VesselBaseMsgData message)
        {
            var msgData = message as VesselPositionMsgData ?? (dynamic) (message as VesselUpdateMsgData);
            if (msgData == null) return;

            if (!LastUpdateDictionary.TryGetValue((Guid)msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FileUpdateIntervalMs)
            {
                if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

                var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.VesselId}.txt");
                if (!File.Exists(path)) return; //didn't found a vessel to rewrite so quit
                var protoVesselLines = FileHandler.ReadFileLines(path);

                string updatedText;
                if (message is VesselPositionMsgData)
                {
                    updatedText = UpdateProtoVesselFileWithNewPositionData(protoVesselLines, msgData);
                }
                else
                {
                    updatedText = UpdateProtoVesselFileWithNewUpdateData(protoVesselLines, msgData);
                }

                FileHandler.WriteToFile(path, updatedText);
                LastUpdateDictionary.AddOrUpdate((Guid)msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);
            }
        }

        /// <summary>
        /// Updates the proto vessel file with the values we received about a position of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewPositionData(string[] protoVesselLines, VesselPositionMsgData msgData)
        {
            var fullText = string.Join(Environment.NewLine, protoVesselLines);

            var regex = new Regex("(lat = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.LatLonAlt[0].ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(lon = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.LatLonAlt[0].ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(alt = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.LatLonAlt[0].ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(hgt = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.HeightFromTerrain.ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(nrm = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.NormalVector[0].ToString(CultureInfo.InvariantCulture)}," +
                                               $"{msgData.NormalVector[1].ToString(CultureInfo.InvariantCulture)}," +
                                               $"{msgData.NormalVector[2].ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(rot = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.SrfRelRotation[0].ToString(CultureInfo.InvariantCulture)}," +
                                               $"{msgData.SrfRelRotation[1].ToString(CultureInfo.InvariantCulture)}," +
                                               $"{msgData.SrfRelRotation[2].ToString(CultureInfo.InvariantCulture)}," +
                                               $"{msgData.SrfRelRotation[3].ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(CoM = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.Com[0].ToString(CultureInfo.InvariantCulture)}," +
                                               $"{msgData.Com[1].ToString(CultureInfo.InvariantCulture)}," +
                                               $"{msgData.Com[2].ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(INC = )(.*)"); //inclination
            fullText = regex.Replace(fullText, $"$1{msgData.Orbit[0].ToString(CultureInfo.InvariantCulture)}");
            regex = new Regex("(ECC = )(.*)"); //eccentricity
            fullText = regex.Replace(fullText, $"$1{msgData.Orbit[1].ToString(CultureInfo.InvariantCulture)}");
            regex = new Regex("(SMA = )(.*)"); //semiMajorAxis
            fullText = regex.Replace(fullText, $"$1{msgData.Orbit[2].ToString(CultureInfo.InvariantCulture)}");
            regex = new Regex("(LAN = )(.*)"); //LAN
            fullText = regex.Replace(fullText, $"$1{msgData.Orbit[3].ToString(CultureInfo.InvariantCulture)}");
            regex = new Regex("(LPE = )(.*)"); //argumentOfPeriapsis
            fullText = regex.Replace(fullText, $"$1{msgData.Orbit[4].ToString(CultureInfo.InvariantCulture)}");
            regex = new Regex("(MNA = )(.*)"); //meanAnomalyAtEpoch
            fullText = regex.Replace(fullText, $"$1{msgData.Orbit[5].ToString(CultureInfo.InvariantCulture)}");
            regex = new Regex("(EPH = )(.*)"); //epoch
            fullText = regex.Replace(fullText, $"$1{msgData.Orbit[6].ToString(CultureInfo.InvariantCulture)}");
            regex = new Regex("(REF = )(.*)"); //referenceBody.flightGlobalsIndex
            fullText = regex.Replace(fullText, $"$1{msgData.Orbit[7].ToString(CultureInfo.InvariantCulture)}");

            return fullText;
        }

        /// <summary>
        /// Updates the proto vessel file with the values we received about a position of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewUpdateData(string[] protoVesselLines, VesselUpdateMsgData msgData)
        {
            var fullText = string.Join(Environment.NewLine, protoVesselLines);

            var regex = new Regex("(name = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.Name}");

            regex = new Regex("(type = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.Type}");

            regex = new Regex("(sit = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.Situation}");

            regex = new Regex("(landed = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.Landed.ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(landedAt = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.LandedAt}");

            regex = new Regex("(displaylandedAt = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.DisplayLandedAt}");

            regex = new Regex("(splashed = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.Splashed.ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(met = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.MissionTime.ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(lct = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.LaunchTime.ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(lastUT = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.LastUt.ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(prst = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.Persistent.ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(ref = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.RefTransformId.ToString(CultureInfo.InvariantCulture)}");

            regex = new Regex("(ctrl = )(.*)");
            fullText = regex.Replace(fullText, $"$1{msgData.Controllable.ToString(CultureInfo.InvariantCulture)}");

            foreach (var actionGroup in msgData.ActionGroups)
            {
                regex = new Regex($"({actionGroup.ActionGroupName} = )(.*)");
                var newValue = string.Concat(actionGroup.ActionGroupName, 
                    " = ",
                    actionGroup.State.ToString(CultureInfo.InvariantCulture), 
                    ", ",
                    actionGroup.Time.ToString(CultureInfo.InvariantCulture));

                fullText = regex.Replace(fullText, $"$1{newValue}");
            }

            return fullText;
        }
    }
}
