using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UniLinq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoRefresh
    {
        private static readonly FieldInfo CrewField = typeof(ProtoVessel).GetField("crew", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void RefreshVesselProto(Vessel vessel)
        {
            vessel.protoVessel.vesselRef = vessel;
            vessel.protoVessel.vesselRef.protoVessel = vessel.protoVessel;
            vessel.protoVessel.vesselID = vessel.id;

            vessel.protoVessel.orbitSnapShot.semiMajorAxis = vessel.orbit.semiMajorAxis;
            vessel.protoVessel.orbitSnapShot.eccentricity = vessel.orbit.eccentricity;
            vessel.protoVessel.orbitSnapShot.inclination = vessel.orbit.inclination;
            vessel.protoVessel.orbitSnapShot.argOfPeriapsis = vessel.orbit.argumentOfPeriapsis;
            vessel.protoVessel.orbitSnapShot.LAN = vessel.orbit.LAN;
            vessel.protoVessel.orbitSnapShot.meanAnomalyAtEpoch = vessel.orbit.meanAnomalyAtEpoch;
            vessel.protoVessel.orbitSnapShot.epoch = vessel.orbit.epoch;
            vessel.protoVessel.orbitSnapShot.ReferenceBodyIndex = FlightGlobals.Bodies.IndexOf(vessel.orbit.referenceBody);

            vessel.protoVessel.refTransform = vessel.referenceTransformId;
            vessel.protoVessel.vesselType = vessel.vesselType;
            vessel.protoVessel.situation = vessel.situation;
            vessel.protoVessel.landed = vessel.Landed;
            vessel.protoVessel.landedAt = vessel.landedAt;
            vessel.protoVessel.displaylandedAt = vessel.displaylandedAt;
            vessel.protoVessel.splashed = vessel.Splashed;
            vessel.protoVessel.vesselName = vessel.vesselName;
            vessel.protoVessel.missionTime = vessel.missionTime;
            vessel.protoVessel.launchTime = vessel.launchTime;
            vessel.protoVessel.lastUT = vessel.lastUT;
            vessel.protoVessel.autoClean = vessel.AutoClean;
            vessel.protoVessel.autoCleanReason = vessel.AutoCleanReason;
            vessel.protoVessel.wasControllable = vessel.IsControllable;

            vessel.protoVessel.CoM = vessel.localCoM;
            vessel.protoVessel.latitude = vessel.latitude;
            vessel.protoVessel.longitude = vessel.longitude;
            vessel.protoVessel.altitude = vessel.altitude;
            vessel.protoVessel.height = vessel.heightFromTerrain;
            vessel.protoVessel.normal = vessel.terrainNormal;
            vessel.protoVessel.rotation = vessel.srfRelRotation;
            vessel.protoVessel.stage = vessel.currentStage;
            vessel.protoVessel.persistent = vessel.isPersistent;

            RefreshParts(vessel);
            RefreshCrew(vessel);
            RefreshActionGroups(vessel);
        }

        private static void RefreshActionGroups(Vessel vessel)
        {
            if (vessel.protoVessel.vesselRef.ActionGroups.groups.Count != vessel.protoVessel.actionGroups.CountValues)
            {
                vessel.protoVessel.actionGroups.ClearData();
                vessel.protoVessel.vesselRef.ActionGroups.Save(vessel.protoVessel.actionGroups);
            }
            else
            {
                for (var i = 0; i < vessel.protoVessel.vesselRef.ActionGroups.groups.Count; i++)
                {
                    var currentVal = vessel.protoVessel.vesselRef.ActionGroups.groups[i];
                    var protoVal = vessel.protoVessel.actionGroups.values[i].value;

                    if (!bool.TryParse(protoVal, out var boolProtoVal) || currentVal != boolProtoVal)
                    {
                        vessel.protoVessel.actionGroups.values[i].value = string.Concat(
                            vessel.protoVessel.vesselRef.ActionGroups.groups[i].ToString(), ", ",
                            vessel.protoVessel.vesselRef.ActionGroups.cooldownTimes[i].ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        private static void RefreshCrew(Vessel vessel)
        {
            if (vessel.crewedParts != vessel.protoVessel.crewedParts ||
                vessel.crewableParts != vessel.protoVessel.crewableParts)
            {
                ((List<ProtoCrewMember>) CrewField.GetValue(vessel.protoVessel)).Clear();
                vessel.protoVessel.RebuildCrewCounts();
            }
        }

        private static void RefreshParts(Vessel vessel)
        {
            if (vessel.parts.Count != vessel.protoVessel.protoPartSnapshots.Count)
            {
                vessel.protoVessel.protoPartSnapshots.Clear();

                foreach (var part in vessel.parts.Where(p => p.State != PartStates.DEAD))
                {
                    vessel.protoVessel.protoPartSnapshots.Add(new ProtoPartSnapshot(part, vessel.protoVessel));
                }

                foreach (var part in vessel.protoVessel.protoPartSnapshots)
                {
                    part.storePartRefs();
                }
            }
            else
            {
                for (var i = 0; i < vessel.parts.Count; i++)
                {
                    if (vessel.parts[i].State == PartStates.DEAD) continue;

                    RefreshPartModules(vessel.parts[i]);
                    RefreshPartResources(vessel.parts[i]);
                }
            }
        }

        private static void RefreshPartModules(Part part)
        {
            if (part.protoPartSnapshot.modules.Count != part.Modules.Count)
            {
                part.protoPartSnapshot.modules.Clear();
                for (var i = 0; i < part.Modules.Count; i++)
                {
                    part.protoPartSnapshot.modules.Add(new ProtoPartModuleSnapshot(part.Modules[i]));
                }
            }
            else
            {
                for (var i = 0; i < part.Modules.Count; i++)
                {
                    var module = part.Modules[i];
                    for (var j = 0; j < module.Fields.Count; j++)
                    {
                        var field = module.Fields[j];
                        var nodeValue = GetConfigNodeVal(field.name, module.snapshot.moduleValues);

                        if (nodeValue?.value != null)
                        {
                            var fieldValueAsString = field.GetStringValue(field.host, false);
                            if (fieldValueAsString != null && nodeValue.value != fieldValueAsString)
                            {
                                nodeValue.value = fieldValueAsString;
                            }
                        }
                    }
                }
            }
        }
        private static void RefreshPartResources(Part part)
        {
            if (part.protoPartSnapshot.resources.Count != part.Resources.Count)
            {
                part.protoPartSnapshot.resources.Clear();
                for (var i = 0; i < part.Resources.Count; i++)
                {
                    part.protoPartSnapshot.resources.Add(new ProtoPartResourceSnapshot(part.Resources[i]));
                }
            }
            else
            {
                for (var i = 0; i < part.protoPartSnapshot.resources.Count; i++)
                {
                    var resourceSnapshot = part.protoPartSnapshot.resources[i];

                    resourceSnapshot.amount = resourceSnapshot.resourceRef.amount;
                    resourceSnapshot.flowState = resourceSnapshot.resourceRef.flowState;
                    resourceSnapshot.maxAmount = resourceSnapshot.resourceRef.maxAmount;
                }
            }
        }

        private static ConfigNode.Value GetConfigNodeVal(string name, ConfigNode node)
        {
            for (var i = 0; i < node.CountValues; i++)
            {
                if (node.values[i].name == name)
                    return node.values[i];
            }

            return null;
        }
    }
}
