using LunaCommon.Message.Data.Vessel;
using Server.Context;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Server.System
{
    /// <summary>
    /// We try to avoid working with protovessels as much as possible as they can be huge files.
    /// This class patches the vessel file with the information messages we receive about a position and other vessel properties.
    /// This way we send the whole vessel definition only when there are parts that have changed 
    /// </summary>
    public class VesselFileUpdater
    {
        #region Constants and update dictionaries

        #region Position

        /// <summary>
        /// Update the vessel files with position data max at a 2,5 seconds interval
        /// </summary>
        private const int FilePositionUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastPositionUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        #endregion

        #region Position

        /// <summary>
        /// Update the vessel files with update data max at a 2,5 seconds interval
        /// </summary>
        private const int FileUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        #endregion

        #region Resources

        /// <summary>
        /// Update the vessel files with resource data max at a 2,5 seconds interval
        /// </summary>
        private const int FileResourcesUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastResourcesUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        #endregion

        #endregion
        /// <summary>
        /// We received a position information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an update vesselproto
        /// </summary>
        public static void WritePositionDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselPositionMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastPositionUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FilePositionUpdateIntervalMs)
            {
                LastPositionUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.VesselId}.txt");
                    if (!File.Exists(path)) return; //didn't found a vessel to rewrite so quit
                    var protoVesselLines = FileHandler.ReadFileLines(path);

                    var updatedText = UpdateProtoVesselFileWithNewPositionData(protoVesselLines, msgData);
                    FileHandler.WriteToFile(path, updatedText);
                });
            }
        }

        /// <summary>
        /// We received a update information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an update vesselproto
        /// </summary>
        public static void WriteUpdateDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselUpdateMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FileUpdateIntervalMs)
            {
                LastUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.VesselId}.txt");
                    if (!File.Exists(path)) return; //didn't found a vessel to rewrite so quit
                    var protoVesselLines = FileHandler.ReadFileLines(path);

                    var updatedText = UpdateProtoVesselFileWithNewUpdateData(protoVesselLines, msgData);
                    FileHandler.WriteToFile(path, updatedText);
                });
            }
        }

        /// <summary>
        /// We received a resource information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an update vesselproto
        /// </summary>
        public static void WriteResourceDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselResourceMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastResourcesUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FileResourcesUpdateIntervalMs)
            {
                LastResourcesUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.VesselId}.txt");
                    if (!File.Exists(path)) return; //didn't found a vessel to rewrite so quit
                    var protoVesselLines = FileHandler.ReadFileLines(path);

                    //var updatedText = UpdateProtoVesselFileWithNewResourceData(protoVesselLines, msgData);
                    //FileHandler.WriteToFile(path, updatedText);
                });
            }
        }

        /// <summary>
        /// Updates the proto vessel file with the values we received about a position of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewPositionData(string[] protoVesselLines, VesselPositionMsgData msgData)
        {
            var fullText = string.Join(Environment.NewLine, protoVesselLines);

            var regex = new Regex("(?<prefix>lat = )(.*)\n");
            var replacement = "${prefix}" + $"{msgData.LatLonAlt[0].ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}";
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>lon = )(.*)\n");
            replacement = "${prefix}" + $"{msgData.LatLonAlt[1].ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}";
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>alt = )(.*)\n");
            replacement = "${prefix}" + $"{msgData.LatLonAlt[2].ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}";
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>hgt = )(.*)\n");
            replacement = "${prefix}" + $"{msgData.HeightFromTerrain.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}";
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>nrm = )(.*)\n");
            replacement = "${prefix}" + $"{msgData.NormalVector[0].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.NormalVector[1].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.NormalVector[2].ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}";
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>rot = )(.*)\n");
            replacement = "${prefix}" + $"{msgData.SrfRelRotation[0].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.SrfRelRotation[1].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.SrfRelRotation[2].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.SrfRelRotation[3].ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}";
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>CoM = )(.*)\n");
            replacement = "${prefix}" + $"{msgData.Com[0].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.Com[1].ToString(CultureInfo.InvariantCulture)}," +
                          $"{msgData.Com[2].ToString(CultureInfo.InvariantCulture)}\r";
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>INC = )(.*)\n"); //inclination
            replacement = "${prefix}" + msgData.Orbit[0].ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>ECC = )(.*)\n"); //eccentricity
            replacement = "${prefix}" + msgData.Orbit[1].ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>SMA = )(.*)\n"); //semiMajorAxis
            replacement = "${prefix}" + msgData.Orbit[2].ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>LAN = )(.*)\n"); //LAN
            replacement = "${prefix}" + msgData.Orbit[3].ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>LPE = )(.*)\n"); //argumentOfPeriapsis
            replacement = "${prefix}" + msgData.Orbit[4].ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>MNA = )(.*)\n"); //meanAnomalyAtEpoch
            replacement = "${prefix}" + msgData.Orbit[5].ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>EPH = )(.*)\n"); //epoch
            replacement = "${prefix}" + msgData.Orbit[6].ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            fullText = regex.Replace(fullText, replacement, 1);

            regex = new Regex("(?<prefix>REF = )(.*)\n"); //referenceBody.flightGlobalsIndex
            replacement = "${prefix}" + msgData.Orbit[7].ToString(CultureInfo.InvariantCulture) + Environment.NewLine;
            fullText = regex.Replace(fullText, replacement, 1);

            return fullText;
        }

        /// <summary>
        /// Updates the proto vessel file with the values we received about a position of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewUpdateData(string[] protoVesselLines, VesselUpdateMsgData msgData)
        {
            var fullText = string.Join(Environment.NewLine, protoVesselLines);

            var regex = new Regex("(?<prefix>name = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Name + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>type = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Type + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>sit = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Situation + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>landed = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Landed.ToString(CultureInfo.InvariantCulture) + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>landedAt = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.LandedAt + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>displaylandedAt = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.DisplayLandedAt + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>splashed = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Splashed.ToString(CultureInfo.InvariantCulture) + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>met = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.MissionTime.ToString(CultureInfo.InvariantCulture) + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>lct = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.LaunchTime.ToString(CultureInfo.InvariantCulture) + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>lastUT = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.LastUt.ToString(CultureInfo.InvariantCulture) + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>prst = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.Persistent.ToString(CultureInfo.InvariantCulture) + Environment.NewLine, 1);

            regex = new Regex("(?<prefix>ref = )(.*)\n");
            fullText = regex.Replace(fullText, "${prefix}" + msgData.RefTransformId.ToString(CultureInfo.InvariantCulture) + Environment.NewLine, 1);

            foreach (var actionGroup in msgData.ActionGroups)
            {
                regex = new Regex($"(?<prefix>{actionGroup.ActionGroupName} = )(.*)\n");
                var newValue = actionGroup.State.ToString(CultureInfo.InvariantCulture) + ", " + actionGroup.Time.ToString(CultureInfo.InvariantCulture);

                fullText = regex.Replace(fullText, "${prefix}" + newValue + Environment.NewLine, 1);
            }

            return fullText;
        }

        /// <summary>
        /// Updates the proto vessel file with the values we received about a position of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewResourceData(string[] protoVesselLines, VesselResourceMsgData msgData)
        {
            throw new NotImplementedException();
        }
    }
}
