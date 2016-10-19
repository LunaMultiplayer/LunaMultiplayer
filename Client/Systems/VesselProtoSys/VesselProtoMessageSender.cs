using System;
using System.IO;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.KerbalReassigner;
using LunaClient.Systems.Network;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
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

        public void SendVesselProtoMessageApplyPosition(ProtoVessel vessel)
        {
            vessel.longitude = FlightGlobals.ActiveVessel.longitude;
            vessel.latitude = FlightGlobals.ActiveVessel.latitude;
            vessel.altitude = FlightGlobals.ActiveVessel.altitude;

            SendVesselProtoMessage(vessel);
        }
        
        public void SendVesselProtoMessage(ProtoVessel vessel)
        {
            //Defend against NaN orbits
            if (VesselHasNaNPosition(vessel))
            {
                Debug.Log("Vessel " + vessel.vesselID + " has NaN position");
                return;
            }

            foreach (var pps in vessel.protoPartSnapshots)
            {
                //Remove tourists from the vessel
                //TODO: Probably this can be done in the CleanUpVesselNode method
                foreach (var pcm in pps.protoModuleCrew.Where(pcm => pcm.type == ProtoCrewMember.KerbalType.Tourist).ToArray())
                    pps.protoModuleCrew.Remove(pcm);
            }

            var vesselNode = new ConfigNode();
            vessel.Save(vesselNode);

            //Clean up the vessel so we send only the important data
            CleanUpVesselNode(vesselNode, vessel.vesselID);

            var vesselBytes = ConfigNodeSerializer.Singleton.Serialize(vesselNode);
            var path = CommonUtil.CombinePaths(Client.KspPath, "GameData", "LunaMultiPlayer","Plugins", "Data", "lastVessel.txt");
            File.WriteAllBytes(path, vesselBytes);

            if (vesselBytes.Length > 0)
            {
                UniverseSyncCache.Singleton.QueueToCache(vesselBytes);
                Debug.Log($"Sending vessel {vessel.vesselID}, Name {vessel.vesselName}, type: {vessel.vesselType}, size: {vesselBytes.Length}");

                SendMessage(new VesselProtoMsgData
                {
                    VesselId = vessel.vesselID,
                    VesselData = vesselBytes
                });
            }
            else
            {
                Debug.LogError("Failed to create byte[] data for " + vessel.vesselID);
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
                        Debug.Log("Dodged actiongroup " + keyName);
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
            var boolValue = input.Substring(0, input.IndexOf(", "));
            var timeValue = input.Substring(input.IndexOf(", ") + 1);
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
