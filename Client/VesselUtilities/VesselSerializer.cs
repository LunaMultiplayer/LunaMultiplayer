using LunaClient.Systems;
using LunaClient.Systems.KerbalReassigner;
using LunaClient.Utilities;
using System;
using System.Linq;

namespace LunaClient.VesselUtilities
{
    public class VesselSerializer
    {
        private static readonly object LockObj = new object();
        private static readonly ConfigNode ConfigNode = new ConfigNode();

        public static ProtoVessel DeserializeVessel(byte[] data)
        {
            try
            {
                var vesselNode = ConfigNodeSerializer.Deserialize(data);
                var configGuid = vesselNode?.GetValue("pid");

                return VesselCommon.CreateSafeProtoVesselFromConfigNode(vesselNode, new Guid(configGuid));
            }
            catch (Exception)
            {
                LunaLog.LogError("[LMP]: Error while deserializing vessel");
                return null;
            }
        }

        public static byte[] SerializeVessel(ProtoVessel protoVessel)
        {
            //Lock this as we are using a shared ConfigNode
            lock (LockObj)
            {
                ConfigNode.ClearData();
                try
                {
                    protoVessel.Save(ConfigNode);
                }
                catch (Exception)
                {
                    LunaLog.LogError("[LMP]: Error while saving vessel");
                    return new byte[0];
                }

                var vesselId = new Guid(ConfigNode.GetValue("pid"));

                //Defend against NaN orbits
                if (VesselHasNaNPosition(ConfigNode))
                {
                    LunaLog.Log($"[LMP]: Vessel {vesselId} has NaN position");
                    return new byte[0];
                }

                //Clean up the vessel so we send only the important data
                CleanUpVesselNode(ConfigNode, vesselId);

                //TODO: Remove tourists from the vessel. This must be done in the CleanUpVesselNode method
                //foreach (var pps in protoVessel.protoPartSnapshots)
                //{
                //    foreach (var pcm in
                //        pps.protoModuleCrew.Where(pcm => pcm.type == ProtoCrewMember.KerbalType.Tourist).ToArray())
                //        pps.protoModuleCrew.Remove(pcm);
                //}

                return ConfigNodeSerializer.Serialize(ConfigNode);
            }
        }

        #region Private methods

        /// <summary>
        /// Here we clean up the node in order to NOT send some of the data (like maneuver nodes, etc)
        /// </summary>
        private static void CleanUpVesselNode(ConfigNode vesselNode, Guid vesselId)
        {
            RemoveManeuverNodesFromProtoVessel(vesselNode);
            FixVesselActionGroupsNodes(vesselNode);
            SystemsContainer.Get<KerbalReassignerSystem>().DodgeKerbals(vesselNode, vesselId);
        }

        #region Config node fixing

        private static bool VesselHasNaNPosition(ConfigNode vesselNode)
        {
            if (vesselNode.GetValue("landed") == "True" || vesselNode.GetValue("splashed") == "True")
            {
                if (double.TryParse(vesselNode.values.GetValue("lat"), out var latitude)
                    && (double.IsNaN(latitude) || double.IsInfinity(latitude)))
                    return true;

                if (double.TryParse(vesselNode.values.GetValue("lon"), out var longitude)
                    && (double.IsNaN(longitude) || double.IsInfinity(longitude)))
                    return true;

                if (double.TryParse(vesselNode.values.GetValue("alt"), out var altitude)
                    && (double.IsNaN(altitude) || double.IsInfinity(altitude)))
                    return true;
            }
            else
            {
                var orbitNode = vesselNode?.GetNode("ORBIT");
                if (orbitNode != null)
                {
                    return orbitNode.values.DistinctNames().Select(v => orbitNode.GetValue(v))
                        .Any(val => double.TryParse(val, out var value) && (double.IsNaN(value) || double.IsInfinity(value)));
                }
            }

            return false;
        }

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
                        LunaLog.Log($"[LMP]: Dodged actiongroup {keyName}");
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
