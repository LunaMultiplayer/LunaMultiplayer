using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.KerbalReassigner;
using LunaClient.Utilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using UniLinq;
using UnityEngine;

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

            //Update the protovessel definition!
            vessel.BackupVessel();

            //Defend against NaN orbits
            if (VesselHasNaNPosition(vessel.protoVessel))
            {
                Debug.Log($"[LMP]: Vessel {vessel.id} has NaN position");
                return;
            }

            foreach (var pps in vessel.protoVessel.protoPartSnapshots)
            {
                //Remove tourists from the vessel
                //TODO: Probably this can be done in the CleanUpVesselNode method
                foreach (var pcm in
                    pps.protoModuleCrew.Where(pcm => pcm.type == ProtoCrewMember.KerbalType.Tourist).ToArray())
                    pps.protoModuleCrew.Remove(pcm);
            }

            var vesselNode = new ConfigNode();
            try
            {
                vessel.protoVessel.Save(vesselNode);
            }
            catch (Exception)
            {
                Debug.LogError("[LMP]: Error while saving vessel");
                return;
            }

            //Clean up the vessel so we send only the important data
            CleanUpVesselNode(vesselNode, vessel.id);

            var vesselBytes = ConfigNodeSerializer.Serialize(vesselNode);
            if (vesselBytes.Length > 0)
            {
                UniverseSyncCache.QueueToCache(vesselBytes);

                SendMessage(new VesselProtoMsgData
                {
                    VesselId = vessel.id,
                    VesselData = vesselBytes
                });
            }
            else
            {
                Debug.LogError($"[LMP]: Failed to create byte[] data for {vessel.id}");
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
            KerbalReassignerSystem.Singleton.DodgeKerbals(vesselNode, vesselId);
        }

        private static bool VesselHasNaNPosition(ProtoVessel pv)
        {
            if (pv.landed || pv.splashed)
            {
                //Ground checks
                if (double.IsNaN(pv.latitude) || double.IsInfinity(pv.latitude) ||
                    double.IsNaN(pv.longitude) || double.IsInfinity(pv.longitude) ||
                    double.IsNaN(pv.altitude) || double.IsInfinity(pv.altitude))
                    return true;
            }
            else
            {
                //Orbit checks
                if (double.IsNaN(pv.orbitSnapShot.argOfPeriapsis) || double.IsInfinity(pv.orbitSnapShot.argOfPeriapsis) ||
                    double.IsNaN(pv.orbitSnapShot.eccentricity) || double.IsInfinity(pv.orbitSnapShot.eccentricity) ||
                    double.IsNaN(pv.orbitSnapShot.epoch) || double.IsInfinity(pv.orbitSnapShot.epoch) ||
                    double.IsNaN(pv.orbitSnapShot.inclination) || double.IsInfinity(pv.orbitSnapShot.inclination) ||
                    double.IsNaN(pv.orbitSnapShot.LAN) || double.IsInfinity(pv.orbitSnapShot.LAN) ||
                    double.IsNaN(pv.orbitSnapShot.meanAnomalyAtEpoch) ||
                    double.IsInfinity(pv.orbitSnapShot.meanAnomalyAtEpoch) ||
                    double.IsNaN(pv.orbitSnapShot.semiMajorAxis) || double.IsInfinity(pv.orbitSnapShot.semiMajorAxis))
                    return true;
            }

            return false;
        }

        #region Config node fixing

        /// <summary>
        /// Removes maneuver nodes from the vessel
        /// </summary>
        private void RemoveManeuverNodesFromProtoVessel(ConfigNode vesselNode)
        {
            var flightPlanNode = vesselNode?.GetNode("FLIGHTPLAN");
            flightPlanNode?.ClearData();
        }

        /// <summary>
        /// Fixes the nodes of the action groups so they are correct
        /// </summary>
        /// <param name="vesselNode"></param>
        private void FixVesselActionGroupsNodes(ConfigNode vesselNode)
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
                        Debug.Log($"[LMP]: Dodged actiongroup {keyName}");
                        actiongroupNode.SetValue(keyName, valueDodge);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the action group was true or false and fix it's planet time
        /// </summary>
        private string DodgeValueIfNeeded(string input)
        {
            var boolValue = input.Substring(0, input.IndexOf(", ", StringComparison.Ordinal));
            var timeValue = input.Substring(input.IndexOf(", ", StringComparison.Ordinal) + 1);
            var vesselPlanetTime = double.Parse(timeValue);
            var currentPlanetTime = Planetarium.GetUniversalTime();
            if (vesselPlanetTime > currentPlanetTime)
                return boolValue + ", " + currentPlanetTime;
            return input;
        }

        #endregion

        #endregion
    }
}
