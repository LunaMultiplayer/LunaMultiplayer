using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.KerbalReassigner;
using LunaClient.Utilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;
using UniLinq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageSender : SubSystem<VesselProtoSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselMessage(Vessel vessel)
        {
            if (vessel == null) return;
            SendVesselMessage(vessel.protoVessel);
        }

        public void SendVesselMessage(ProtoVessel protoVessel)
        {
            if (protoVessel == null) return;
            TaskFactory.StartNew(() => PrepareAndSendProtoVessel(protoVessel));
        }

        public void SendVesselMessage(IEnumerable<Vessel> vessels)
        {
            foreach (var vessel in vessels)
            {
                SendVesselMessage(vessel);
            }
        }

        /// <summary>
        /// This method prepares the protovessel class and send the message, it's intended to be run in another thread
        /// </summary>
        private void PrepareAndSendProtoVessel(ProtoVessel protoVessel)
        {
            var vesselNode = new ConfigNode();
            try
            {
                protoVessel.Save(vesselNode);
            }
            catch (Exception)
            {
                LunaLog.LogError("[LMP]: Error while saving vessel");
                return;
            }

            var vesselId = new Guid(vesselNode.GetValue("pid"));

            //Defend against NaN orbits
            if (VesselHasNaNPosition(vesselNode))
            {
                LunaLog.Log($"[LMP]: Vessel {vesselId} has NaN position");
                return;
            }

            //Clean up the vessel so we send only the important data
            CleanUpVesselNode(vesselNode, vesselId);

            //TODO: Remove tourists from the vessel. This must be done in the CleanUpVesselNode method
            //foreach (var pps in protoVessel.protoPartSnapshots)
            //{
            //    foreach (var pcm in
            //        pps.protoModuleCrew.Where(pcm => pcm.type == ProtoCrewMember.KerbalType.Tourist).ToArray())
            //        pps.protoModuleCrew.Remove(pcm);
            //}

            var vesselBytes = ConfigNodeSerializer.Serialize(vesselNode);
            if (vesselBytes.Length > 0)
            {
                UniverseSyncCache.QueueToCache(vesselBytes);

                SendMessage(new VesselProtoMsgData
                {
                    VesselId = vesselId,
                    VesselData = vesselBytes
                });
            }
            else
            {
                LunaLog.LogError($"[LMP]: Failed to create byte[] data for {protoVessel.vesselID}");
            }
        }

        #region Private methods

        /// <summary>
        /// Here we clean up the node in order to NOT send some of the data (like maneuver nodes, etc)
        /// </summary>
        private void CleanUpVesselNode(ConfigNode vesselNode, Guid vesselId)
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
