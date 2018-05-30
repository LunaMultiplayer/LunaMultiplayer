using LunaClient.Base;
using LunaClient.Systems.VesselEvaSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace LunaClient.VesselStore
{
    /// <summary>
    /// This class holds a dictionary with all the vessel definitions that we received either when 2 vessels are docked or some vessel part changes or 
    /// a vessel gets a change. It's a way of unifying the data-storage of VesselChangeSystem and VesselProtoSystem and also be able to update a 
    /// vessel definition when a docking happens or to acces this dictionary from the VesselRemoveSystem
    /// </summary>
    public class VesselsProtoStore
    {
        public static ConcurrentDictionary<Guid, VesselProtoUpdate> AllPlayerVessels { get; } =
            new ConcurrentDictionary<Guid, VesselProtoUpdate>();

        /// <summary>
        /// In this method we get the new vessel data and set it to the dictionary of all the player vessels.
        /// </summary>
        public static void HandleVesselProtoData(byte[] vesselData, int numBytes, Guid vesselId)
        {
            SystemBase.TaskFactory.StartNew(() =>
            {
                if (AllPlayerVessels.TryGetValue(vesselId, out var vesselUpdate))
                {
                    vesselUpdate.Update(vesselData, numBytes, vesselId);
                }
                else
                {
                    AllPlayerVessels.TryAdd(vesselId, new VesselProtoUpdate(vesselData, numBytes, vesselId));
                }
            });
        }

        /// <summary>
        /// Clears the whole system
        /// </summary>
        public static void ClearSystem()
        {
            AllPlayerVessels.Clear();
        }

        /// <summary>
        /// Removes a vessel from the proto system. Bar in mind that if we receive a protovessel msg after this method is called it will be reloaded
        /// </summary>
        public static void RemoveVessel(Guid vesselId)
        {
            AllPlayerVessels.TryRemove(vesselId, out var _);
        }

        /// <summary>
        /// Check in the store if there's a vessel with that part in it's protovessel. If so it returns that vessel
        /// </summary>
        public static Vessel GetVesselByPartId(uint flightId)
        {
            var keys = AllPlayerVessels.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                if (AllPlayerVessels.TryGetValue(keys[i], out var vesselProtoUpd))
                {
                    if (vesselProtoUpd.VesselParts.ContainsKey(flightId))
                        return vesselProtoUpd.Vessel;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a vessel manually to the dictionary. Use this to add your own spawned vessels.
        /// </summary>
        public static void AddOrUpdateVesselToDictionary(Vessel vessel)
        {
            var ownVesselData = VesselSerializer.SerializeVessel(vessel.protoVessel);
            if (ownVesselData.Length > 0)
            {
                var newProtoUpdate = new VesselProtoUpdate(ownVesselData, ownVesselData.Length, vessel.id);
                AllPlayerVessels.AddOrUpdate(vessel.id, newProtoUpdate, (key, existingVal) => newProtoUpdate);
            }
        }

        /// <summary>
        /// Raw updates a vessel on the dictionary. Use this to update vessels that you have the update/unloaded update lock.
        /// </summary>
        public static void RawUpdateVesselProtoData(byte[] vesselData, int numBytes, Guid vesselId)
        {
            if (AllPlayerVessels.TryGetValue(vesselId, out var vesselProtoUpd))
            {
                vesselProtoUpd.Update(vesselData, numBytes, vesselId);
            }
        }

        public static void UpdateVesselProtoPosition(VesselPositionMsgData vesselPositionMsgData)
        {
            if (AllPlayerVessels.TryGetValue(vesselPositionMsgData.VesselId, out var vesselProtoUpd))
            {
                if (vesselProtoUpd.ProtoVessel == null) return;

                vesselProtoUpd.ProtoVessel.latitude = vesselPositionMsgData.LatLonAlt[0];
                vesselProtoUpd.ProtoVessel.longitude = vesselPositionMsgData.LatLonAlt[1];
                vesselProtoUpd.ProtoVessel.altitude = vesselPositionMsgData.LatLonAlt[2];
                vesselProtoUpd.ProtoVessel.height = vesselPositionMsgData.HeightFromTerrain;

                vesselProtoUpd.ProtoVessel.normal.x = (float)vesselPositionMsgData.NormalVector[0];
                vesselProtoUpd.ProtoVessel.normal.y = (float)vesselPositionMsgData.NormalVector[1];
                vesselProtoUpd.ProtoVessel.normal.z = (float)vesselPositionMsgData.NormalVector[2];

                vesselProtoUpd.ProtoVessel.rotation.x = vesselPositionMsgData.SrfRelRotation[0];
                vesselProtoUpd.ProtoVessel.rotation.y = vesselPositionMsgData.SrfRelRotation[1];
                vesselProtoUpd.ProtoVessel.rotation.z = vesselPositionMsgData.SrfRelRotation[2];
                vesselProtoUpd.ProtoVessel.rotation.w = vesselPositionMsgData.SrfRelRotation[3];

                if (vesselProtoUpd.ProtoVessel.orbitSnapShot != null)
                {
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.inclination = vesselPositionMsgData.Orbit[0];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.eccentricity = vesselPositionMsgData.Orbit[1];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.semiMajorAxis = vesselPositionMsgData.Orbit[2];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.LAN = vesselPositionMsgData.Orbit[3];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.argOfPeriapsis = vesselPositionMsgData.Orbit[4];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.meanAnomalyAtEpoch = vesselPositionMsgData.Orbit[5];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.epoch = vesselPositionMsgData.Orbit[6];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.ReferenceBodyIndex = (int)vesselPositionMsgData.Orbit[7];
                }
            }
        }

        public static void UpdateVesselProtoValues(VesselUpdateMsgData msgData)
        {
            if (AllPlayerVessels.TryGetValue(msgData.VesselId, out var vesselProtoUpd))
            {
                if (vesselProtoUpd.ProtoVessel == null) return;

                vesselProtoUpd.ProtoVessel.vesselName = msgData.Name;
                vesselProtoUpd.ProtoVessel.vesselType = (VesselType)Enum.Parse(typeof(VesselType), msgData.Type);
                vesselProtoUpd.ProtoVessel.distanceTraveled = msgData.DistanceTraveled;
                vesselProtoUpd.ProtoVessel.situation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), msgData.Situation);
                vesselProtoUpd.ProtoVessel.landed = msgData.Landed;
                vesselProtoUpd.ProtoVessel.landedAt = msgData.LandedAt;
                vesselProtoUpd.ProtoVessel.displaylandedAt = msgData.DisplayLandedAt;
                vesselProtoUpd.ProtoVessel.splashed = msgData.Splashed;
                vesselProtoUpd.ProtoVessel.missionTime = msgData.MissionTime;
                vesselProtoUpd.ProtoVessel.launchTime = msgData.LaunchTime;
                vesselProtoUpd.ProtoVessel.lastUT = msgData.LastUt;
                vesselProtoUpd.ProtoVessel.persistent = msgData.Persistent;
                vesselProtoUpd.ProtoVessel.refTransform = msgData.RefTransformId;
                vesselProtoUpd.ProtoVessel.autoClean = msgData.AutoClean;
                vesselProtoUpd.ProtoVessel.autoCleanReason = msgData.AutoCleanReason;
                vesselProtoUpd.ProtoVessel.wasControllable = msgData.WasControllable;
                vesselProtoUpd.ProtoVessel.stage = msgData.Stage;
                vesselProtoUpd.ProtoVessel.CoM.x = msgData.Com[0];
                vesselProtoUpd.ProtoVessel.CoM.y = msgData.Com[1];
                vesselProtoUpd.ProtoVessel.CoM.z = msgData.Com[2];

                vesselProtoUpd.ProtoVessel.actionGroups.ClearValues();
                for (var i = 0; i < vesselProtoUpd.ProtoVessel.actionGroups.values.Count; i++)
                {
                    vesselProtoUpd.ProtoVessel.actionGroups.AddValue(msgData.ActionGroups[i].ActionGroupName, msgData.ActionGroups[i].State + ", " + msgData.ActionGroups[i].Time);
                }
            }
        }

        public static void UpdateVesselProtoPartFairing(VesselFairingMsgData msgData)
        {
            if (AllPlayerVessels.TryGetValue(msgData.VesselId, out var vesselProtoUpd))
            {
                if (vesselProtoUpd.ProtoVessel == null) return;

                var part = VesselCommon.FindProtoPartInProtovessel(vesselProtoUpd.ProtoVessel, msgData.PartFlightId);
                if (part != null)
                {
                    var module = VesselCommon.FindProtoPartModuleInProtoPart(part, "ModuleProceduralFairing");
                    module?.moduleValues.SetValue("fsm", "st_flight_deployed");
                    module?.moduleValues.RemoveNodesStartWith("XSECTION");
                }
            }
        }

        public static void UpdateVesselProtoPartModules(VesselPartSyncMsgData msgData)
        {
            if (AllPlayerVessels.TryGetValue(msgData.VesselId, out var vesselProtoUpd))
            {
                if (vesselProtoUpd.ProtoVessel == null) return;

                var partSnapshot = VesselCommon.FindProtoPartInProtovessel(vesselProtoUpd.ProtoVessel, msgData.PartFlightId);
                if (partSnapshot != null)
                {
                    var module = VesselCommon.FindProtoPartModuleInProtoPart(partSnapshot, msgData.ModuleName);
                    module?.moduleValues.SetValue(msgData.FieldName, msgData.Value);
                }
            }
        }

        public static void UpdateVesselProtoResources(VesselResourceMsgData msgData)
        {
            if (AllPlayerVessels.TryGetValue(msgData.VesselId, out var vesselProtoUpd))
            {
                if (vesselProtoUpd.ProtoVessel == null) return;

                for (var i = 0; i < msgData.ResourcesCount; i++)
                {
                    var resource = msgData.Resources[i];

                    if (resource == null) continue;

                    var partSnapshot = VesselCommon.FindProtoPartInProtovessel(vesselProtoUpd.ProtoVessel, resource.PartFlightId);
                    var resourceSnapshot = VesselCommon.FindResourceInProtoPart(partSnapshot, resource.ResourceName);
                    if (resourceSnapshot != null)
                    {
                        resourceSnapshot.amount = resource.Amount;
                        resourceSnapshot.flowState = resource.FlowState;
                    }
                }
            }
        }

        public static void UpdateVesselProtoEvaFsm(VesselEvaMsgData msgData)
        {
            if (AllPlayerVessels.TryGetValue(msgData.VesselId, out var vesselProtoUpd))
            {
                VesselEvaSystem.Singleton.UpdateFsmStateInProtoVessel(vesselProtoUpd.ProtoVessel, msgData.NewState, msgData.LastBoundStep);
            }
        }

        public static void UpdateVesselProtoFlightState(VesselFlightStateMsgData msgData)
        {
            if (AllPlayerVessels.TryGetValue(msgData.VesselId, out var vesselProtoUpd))
            {
                VesselFlightStateSystem.Singleton.UpdateFlightStateInProtoVessel(vesselProtoUpd.ProtoVessel, msgData);
            }
        }
    }
}
