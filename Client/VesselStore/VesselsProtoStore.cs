using LunaClient.Base;
using LunaClient.Systems.VesselPositionSys;
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
        public static void HandleVesselProtoData(byte[] vesselData, int numBytes, Guid vesselId, bool runSyncronously, int vesselSituation = int.MinValue)
        {
            if (runSyncronously)
                HandleData();
            else
                SystemBase.TaskFactory.StartNew(HandleData);

            void HandleData()
            {
                if (AllPlayerVessels.TryGetValue(vesselId, out var vesselUpdate))
                {
                    vesselUpdate.Update(vesselData, numBytes, vesselId, vesselSituation);
                }
                else
                {
                    AllPlayerVessels.TryAdd(vesselId, new VesselProtoUpdate(vesselData, numBytes, vesselId));
                }
            }
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
        public static void AddVesselToDictionary(Vessel vessel)
        {
            var ownVesselData = VesselSerializer.SerializeVessel(vessel.protoVessel);
            if (ownVesselData.Length > 0)
            {
                var newProtoUpdate = new VesselProtoUpdate(ownVesselData, ownVesselData.Length, FlightGlobals.ActiveVessel.id);
                AllPlayerVessels.AddOrUpdate(FlightGlobals.ActiveVessel.id, newProtoUpdate, (key, existingVal) => newProtoUpdate);
            }
        }

        public static void UpdateVesselProtoPosition(VesselPositionUpdate vesselPositionUpdate)
        {
            if (AllPlayerVessels.TryGetValue(vesselPositionUpdate.VesselId, out var vesselProtoUpd))
            {
                vesselProtoUpd.ProtoVessel.latitude = vesselPositionUpdate.LatLonAlt[0];
                vesselProtoUpd.ProtoVessel.longitude = vesselPositionUpdate.LatLonAlt[0];
                vesselProtoUpd.ProtoVessel.altitude = vesselPositionUpdate.LatLonAlt[0];
                vesselProtoUpd.ProtoVessel.height = vesselPositionUpdate.HeightFromTerrain;

                vesselProtoUpd.ProtoVessel.normal[0] = vesselPositionUpdate.Normal[0];
                vesselProtoUpd.ProtoVessel.normal[1] = vesselPositionUpdate.Normal[1];
                vesselProtoUpd.ProtoVessel.normal[2] = vesselPositionUpdate.Normal[2];

                vesselProtoUpd.ProtoVessel.rotation[0] = vesselPositionUpdate.SrfRelRotation[0];
                vesselProtoUpd.ProtoVessel.rotation[1] = vesselPositionUpdate.SrfRelRotation[1];
                vesselProtoUpd.ProtoVessel.rotation[2] = vesselPositionUpdate.SrfRelRotation[2];
                vesselProtoUpd.ProtoVessel.rotation[3] = vesselPositionUpdate.SrfRelRotation[3];

                vesselProtoUpd.ProtoVessel.CoM[0] = vesselPositionUpdate.CoM[0];
                vesselProtoUpd.ProtoVessel.CoM[1] = vesselPositionUpdate.CoM[1];
                vesselProtoUpd.ProtoVessel.CoM[2] = vesselPositionUpdate.CoM[2];

                if (vesselProtoUpd.ProtoVessel.orbitSnapShot != null)
                {
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.inclination = vesselPositionUpdate.Orbit[0];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.eccentricity = vesselPositionUpdate.Orbit[1];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.semiMajorAxis = vesselPositionUpdate.Orbit[2];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.LAN = vesselPositionUpdate.Orbit[3];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.argOfPeriapsis = vesselPositionUpdate.Orbit[4];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.meanAnomalyAtEpoch = vesselPositionUpdate.Orbit[5];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.epoch = vesselPositionUpdate.Orbit[6];
                    vesselProtoUpd.ProtoVessel.orbitSnapShot.ReferenceBodyIndex = (int) vesselPositionUpdate.Orbit[7];
                }
            }
        }

        public static void UpdateVesselProtoValues(VesselUpdateMsgData msgData)
        {
            if (AllPlayerVessels.TryGetValue(msgData.VesselId, out var vesselProtoUpd))
            {
                vesselProtoUpd.ProtoVessel.vesselName = msgData.Name;
                vesselProtoUpd.ProtoVessel.vesselType = (VesselType)Enum.Parse(typeof(VesselType), msgData.Type);
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
            }
        }
    }
}
