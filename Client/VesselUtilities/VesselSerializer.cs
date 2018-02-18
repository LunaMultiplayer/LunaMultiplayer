using LunaClient.Utilities;
using System;

namespace LunaClient.VesselUtilities
{
    public class VesselSerializer
    {
        public static ProtoVessel DeserializeVessel(byte[] data, int numBytes)
        {
            try
            {
                var vesselNode = ConfigNodeSerializer.Deserialize(data, numBytes);
                var configGuid = vesselNode?.GetValue("pid");

                return VesselCommon.CreateSafeProtoVesselFromConfigNode(vesselNode, new Guid(configGuid));
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing vessel: {e}");
                return null;
            }
        }

        public static byte[] SerializeVessel(ProtoVessel protoVessel)
        {
            return PreSerializationChecks(protoVessel, out var configNode) ? ConfigNodeSerializer.Serialize(configNode) : new byte[0];
        }

        /// <summary>
        /// Serializes a vessel to a previous preallocated array (avoids garbage generation)
        /// </summary>
        public static void SerializeVesselToArray(ProtoVessel protoVessel, byte[] data, out int numBytes)
        {
            if (PreSerializationChecks(protoVessel, out var configNode))
            {
                ConfigNodeSerializer.SerializeToArray(configNode, data, out numBytes);
            }
            else
            {
                numBytes = 0;
            }
        }

        #region Private methods

        private static bool PreSerializationChecks(ProtoVessel protoVessel, out ConfigNode configNode)
        {
            configNode = new ConfigNode();

            if (protoVessel == null)
            {
                LunaLog.LogError("[LMP]: Cannot serialize a null protovessel");
                return false;
            }

            try
            {
                protoVessel.Save(configNode);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while saving vessel: {e}");
                return false;
            }

            var vesselId = new Guid(configNode.GetValue("pid"));

            //Defend against NaN orbits
            if (VesselCommon.VesselHasNaNPosition(configNode))
            {
                LunaLog.LogError($"[LMP]: Vessel {vesselId} has NaN position");
                return false;
            }

            //Clean up the vessel so we send only the important data
            CleanUpVesselNode(configNode, vesselId);

            //TODO: Remove tourists from the vessel. This must be done in the CleanUpVesselNode method
            //foreach (var pps in protoVessel.protoPartSnapshots)
            //{
            //    foreach (var pcm in
            //        pps.protoModuleCrew.Where(pcm => pcm.type == ProtoCrewMember.KerbalType.Tourist).ToArray())
            //        pps.protoModuleCrew.Remove(pcm);
            //}

            return true;
        }

        /// <summary>
        /// Here we clean up the node in order to NOT send some of the data (like maneuver nodes, etc)
        /// </summary>
        private static void CleanUpVesselNode(ConfigNode vesselNode, Guid vesselId)
        {
            RemoveManeuverNodesFromProtoVessel(vesselNode);
            FixVesselActionGroupsNodes(vesselNode);
        }

        #region Config node fixing

        /// <summary>
        /// Removes maneuver nodes from the vessel
        /// </summary>
        private static void RemoveManeuverNodesFromProtoVessel(ConfigNode vesselNode)
        {
            var flightPlanNode = vesselNode?.GetNode("FLIGHTPLAN");
            flightPlanNode?.ClearData();
        }

        /// <summary>
        /// Fixes the nodes of the action groups so they are correct
        /// </summary>
        /// <param name="vesselNode"></param>
        private static void FixVesselActionGroupsNodes(ConfigNode vesselNode)
        {
            var actiongroupNode = vesselNode?.GetNode("ACTIONGROUPS");
            if (actiongroupNode != null)
            {
                foreach (var keyName in actiongroupNode.values.DistinctNames())
                {
                    var valueCurrent = actiongroupNode.GetValue(keyName);
                    var valueDodge = DodgeValueIfNeeded(valueCurrent);
                    if (valueCurrent != valueDodge)
                    {
                        actiongroupNode.SetValue(keyName, valueDodge);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the action group was true or false and fix it's planet time
        /// </summary>
        private static string DodgeValueIfNeeded(string input)
        {
            var boolValue = input.Substring(0, input.IndexOf(", ", StringComparison.Ordinal));
            var timeValue = input.Substring(input.IndexOf(", ", StringComparison.Ordinal) + 1);
            var vesselPlanetTime = double.Parse(timeValue);
            var currentPlanetTime = Planetarium.GetUniversalTime();

            return vesselPlanetTime > currentPlanetTime ? $"{boolValue}, {currentPlanetTime}" : input;
        }

        #endregion

        #endregion
    }
}
