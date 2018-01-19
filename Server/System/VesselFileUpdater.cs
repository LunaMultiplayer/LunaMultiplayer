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
            var msgData = message as VesselPositionMsgData ?? (dynamic)(message as VesselUpdateMsgData);
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

            var regex = new Regex("(?<prefix>lat = )(.*)");
            var replacement = "${prefix}" + $"{msgData.LatLonAlt[0].ToString(CultureInfo.InvariantCulture)}";
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>lon = )(.*)");
            replacement = "${prefix}" + $"{msgData.LatLonAlt[1].ToString(CultureInfo.InvariantCulture)}";
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>alt = )(.*)");
            replacement = "${prefix}" + $"{msgData.LatLonAlt[2].ToString(CultureInfo.InvariantCulture)}";
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>hgt = )(.*)");
            replacement = "${prefix}" + $"{msgData.HeightFromTerrain.ToString(CultureInfo.InvariantCulture)}";
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>nrm = )(.*)");
            replacement = "${prefix}" + $"{msgData.NormalVector[0].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.NormalVector[1].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.NormalVector[2].ToString(CultureInfo.InvariantCulture)}";
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>rot = )(.*)");
            replacement = "${prefix}" + $"{msgData.SrfRelRotation[0].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.SrfRelRotation[1].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.SrfRelRotation[2].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.SrfRelRotation[3].ToString(CultureInfo.InvariantCulture)}";
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>CoM = )(.*)");
            replacement = "${prefix}" + $"{msgData.Com[0].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.Com[1].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.Com[2].ToString(CultureInfo.InvariantCulture)}";
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>INC = )(.*)"); //inclination
            replacement = "${prefix}" + msgData.Orbit[0].ToString(CultureInfo.InvariantCulture);
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>ECC = )(.*)"); //eccentricity
            replacement = "${prefix}" + msgData.Orbit[1].ToString(CultureInfo.InvariantCulture);
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>SMA = )(.*)"); //semiMajorAxis
            replacement = "${prefix}" + msgData.Orbit[2].ToString(CultureInfo.InvariantCulture);
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>LAN = )(.*)"); //LAN
            replacement = "${prefix}" + msgData.Orbit[3].ToString(CultureInfo.InvariantCulture);
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>LPE = )(.*)"); //argumentOfPeriapsis
            replacement = "${prefix}" + msgData.Orbit[4].ToString(CultureInfo.InvariantCulture);
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>MNA = )(.*)"); //meanAnomalyAtEpoch
            replacement = "${prefix}" + msgData.Orbit[5].ToString(CultureInfo.InvariantCulture);
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>EPH = )(.*)"); //epoch
            replacement = "${prefix}" + msgData.Orbit[6].ToString(CultureInfo.InvariantCulture);
            fullText = regex.Replace(fullText, replacement);

            regex = new Regex("(?<prefix>REF = )(.*)"); //referenceBody.flightGlobalsIndex
            replacement = "${prefix}" + msgData.Orbit[7].ToString(CultureInfo.InvariantCulture);
            fullText = regex.Replace(fullText, replacement);

            return fullText;
        }

        /// <summary>
        /// Updates the proto vessel file with the values we received about a position of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewUpdateData(string[] protoVesselLines, VesselUpdateMsgData msgData)
        {
            var fullText = string.Join(Environment.NewLine, protoVesselLines);

            var regex = new Regex("(?<prefix>name = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Name);

            regex = new Regex("(?<prefix>type = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Type);

            regex = new Regex("(?<prefix>sit = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Situation);

            regex = new Regex("(?<prefix>landed = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Landed.ToString(CultureInfo.InvariantCulture));

            regex = new Regex("(?<prefix>landedAt = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.LandedAt);

            regex = new Regex("(?<prefix>displaylandedAt = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.DisplayLandedAt);

            regex = new Regex("(?<prefix>splashed = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Splashed.ToString(CultureInfo.InvariantCulture));

            regex = new Regex("(?<prefix>met = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.MissionTime.ToString(CultureInfo.InvariantCulture));

            regex = new Regex("(?<prefix>lct = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.LaunchTime.ToString(CultureInfo.InvariantCulture));

            regex = new Regex("(?<prefix>lastUT = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.LastUt.ToString(CultureInfo.InvariantCulture));

            regex = new Regex("(?<prefix>prst = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Persistent.ToString(CultureInfo.InvariantCulture));

            regex = new Regex("(?<prefix>ref = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.RefTransformId.ToString(CultureInfo.InvariantCulture));

            regex = new Regex("(?<prefix>ctrl = )(.*)");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Controllable.ToString(CultureInfo.InvariantCulture));

            foreach (var actionGroup in msgData.ActionGroups)
            {
                regex = new Regex($"(?<prefix>{actionGroup.ActionGroupName} = )(.*)");
                var newValue = actionGroup.State.ToString(CultureInfo.InvariantCulture) + ", " + actionGroup.Time.ToString(CultureInfo.InvariantCulture);

                fullText = regex.Replace(fullText, "${prefix}" + newValue);
            }

            return fullText;
        }
    }
}
