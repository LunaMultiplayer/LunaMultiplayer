using System.Collections.Generic;
using System.Reflection;
using UniLinq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoRefresh
    {
        private static readonly FieldInfo CrewField = typeof(ProtoVessel).GetField("crew", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void RefreshVesselProto(Vessel vessel)
        {
            var proto = vessel.protoVessel;

            proto.vesselRef = vessel;

            proto.orbitSnapShot.semiMajorAxis = vessel.orbit.semiMajorAxis;
            proto.orbitSnapShot.eccentricity = vessel.orbit.eccentricity;
            proto.orbitSnapShot.inclination = vessel.orbit.inclination;
            proto.orbitSnapShot.argOfPeriapsis = vessel.orbit.argumentOfPeriapsis;
            proto.orbitSnapShot.LAN = vessel.orbit.LAN;
            proto.orbitSnapShot.meanAnomalyAtEpoch = vessel.orbit.meanAnomalyAtEpoch;
            proto.orbitSnapShot.epoch = vessel.orbit.epoch;
            proto.orbitSnapShot.ReferenceBodyIndex = FlightGlobals.Bodies.IndexOf(vessel.orbit.referenceBody);

            proto.vesselID = vessel.id;
            proto.refTransform = vessel.referenceTransformId;
            proto.vesselType = vessel.vesselType;
            proto.situation = vessel.situation;
            proto.landed = vessel.Landed;
            proto.landedAt = vessel.landedAt;
            proto.displaylandedAt = vessel.displaylandedAt;
            proto.splashed = vessel.Splashed;
            proto.vesselName = vessel.vesselName;
            proto.missionTime = vessel.missionTime;
            proto.launchTime = vessel.launchTime;
            proto.lastUT = vessel.lastUT;
            proto.autoClean = vessel.AutoClean;
            proto.autoCleanReason = vessel.AutoCleanReason;
            proto.wasControllable = vessel.IsControllable;

            if (vessel.parts.Count != proto.protoPartSnapshots.Count)
            {
                proto.protoPartSnapshots.Clear();
                
                foreach (var part in vessel.parts.Where(p=> p.State != PartStates.DEAD))
                {
                    proto.protoPartSnapshots.Add(new ProtoPartSnapshot(part, proto));
                }

                foreach (var part in proto.protoPartSnapshots)
                {
                    part.storePartRefs();
                }
            }

            if (vessel.crewedParts != proto.crewedParts || vessel.crewableParts != proto.crewableParts)
            {
                ((List<ProtoCrewMember>)CrewField.GetValue(proto)).Clear();
                proto.RebuildCrewCounts();
            }

            proto.CoM = vessel.localCoM;
            proto.latitude = vessel.latitude;
            proto.longitude = vessel.longitude;
            proto.altitude = vessel.altitude;
            proto.height = vessel.heightFromTerrain;
            proto.normal = vessel.terrainNormal;
            proto.rotation = vessel.srfRelRotation;
            proto.stage = vessel.currentStage;
            proto.persistent = vessel.isPersistent;
            proto.vesselRef.protoVessel = proto;
            
            proto.actionGroups.ClearData();
            proto.vesselRef.ActionGroups.Save(proto.actionGroups);
        }
    }
}
