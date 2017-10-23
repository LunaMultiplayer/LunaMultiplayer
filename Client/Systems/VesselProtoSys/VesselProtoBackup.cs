using System.Collections.Generic;

namespace LunaClient.Systems.VesselProtoSys
{
    /// <summary>
    /// We cannot call vessel.BackupVessel() as it's not thread safe. 
    /// Therefore I just copy pasted the method and edited the parts that bother.
    /// It's dirty as fuck but that's how the code was decompiled...
    /// </summary>
    public class VesselProtoBackup
    {
        private Dictionary<Part, string> PartTransformNames { get; } = new Dictionary<Part, string>();

        /// <summary>
        /// Prepare a backup. This method copies the transform names as the Transform class cannot be accesed from another thread
        /// Must be called from the Unity thread!
        /// </summary>
        /// <param name="parts"></param>
        public void PrepareBackup(IEnumerable<Part> parts)
        {
            foreach (var part in parts)
            {
                PartTransformNames.Add(part, part.transform.name);
            }
        }

        /// <summary>
        /// Do the backup
        /// </summary>
        public ProtoVessel BackupVessel(Vessel vessel)
        {
            var proto = new ProtoVessel(new ConfigNode(), HighLogic.CurrentGame);
            proto.vesselRef = vessel;
            proto.protoPartSnapshots = new List<ProtoPartSnapshot>();
            proto.vesselStateValues = new Dictionary<string, KSPParseable>();
            proto.orbitSnapShot = new OrbitSnapshot(vessel.orbit);

            //This field is private so we need dirty reflection to set it...
            //proto.crew = new List<ProtoCrewMember>();
            proto.GetType().GetField("crew", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .SetValue(proto, new List<ProtoCrewMember>());

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
            foreach (var item in vessel.parts)
            {
                if (item.State != PartStates.DEAD)
                {
                    //Call our own mapper as otherwise it will fail miserably
                    //proto.protoPartSnapshots.Add(new ProtoPartSnapshot(item, proto));
                    proto.protoPartSnapshots.Add(CreatePartSnapshot(proto, item));
                }
            }
            foreach (var part in proto.protoPartSnapshots)
            {
                part.storePartRefs();
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
            proto.actionGroups = new ConfigNode();
            vessel.ActionGroups.Save(proto.actionGroups);
            proto.discoveryInfo = new ConfigNode();
            vessel.DiscoveryInfo.Save(proto.discoveryInfo);
            proto.flightPlan = new ConfigNode();
            if (vessel.PatchedConicsAttached)
            {
                vessel.flightPlanNode.ClearData();
                vessel.patchedConicSolver.Save(vessel.flightPlanNode);
            }
            vessel.flightPlanNode.CopyTo(proto.flightPlan);
            proto.targetInfo = new ProtoTargetInfo(vessel.targetObject);
            proto.waypointInfo = new ProtoWaypointInfo(vessel.navigationWaypoint);
            proto.ctrlState = new ConfigNode();
            vessel.ctrlState.Save(proto.ctrlState);
            proto.SaveVesselModules();
            vessel.OnSaveFlightState(proto.vesselStateValues);
            proto.RebuildCrewCounts();

            return proto;
        }

        private ProtoPartSnapshot CreatePartSnapshot(ProtoVessel proto, Part part)
        {
            var protoPartSnapshot = new ProtoPartSnapshot(new ConfigNode(), proto, HighLogic.CurrentGame);
            protoPartSnapshot.pVesselRef = proto;
            protoPartSnapshot.partRef = part;
            protoPartSnapshot.partRef.onBackup();
            protoPartSnapshot.partStateValues = new Dictionary<string, KSPParseable>();
            protoPartSnapshot.modules = new List<ProtoPartModuleSnapshot>();
            protoPartSnapshot.resources = new List<ProtoPartResourceSnapshot>();
            protoPartSnapshot.partName = part.partInfo.name;
            protoPartSnapshot.craftID = part.craftID;
            protoPartSnapshot.flightID = part.flightID;
            protoPartSnapshot.missionID = part.missionID;
            protoPartSnapshot.launchID = part.launchID;
            protoPartSnapshot.partInfo = PartLoader.getPartInfoByName(protoPartSnapshot.partName);
            protoPartSnapshot.position = part.orgPos;
            protoPartSnapshot.rotation = part.orgRot;
            protoPartSnapshot.mirror = part.mirrorVector;
            protoPartSnapshot.symMethod = part.symMethod;
            protoPartSnapshot.inverseStageIndex = part.inverseStage;
            protoPartSnapshot.resourcePriorityOffset = part.resourcePriorityOffset;
            protoPartSnapshot.defaultInverseStage = part.defaultInverseStage;
            protoPartSnapshot.seqOverride = part.manualStageOffset;
            protoPartSnapshot.inStageIndex = part.inStageIndex;
            protoPartSnapshot.separationIndex = part.separationIndex;
            protoPartSnapshot.customPartData = part.customPartData;
            protoPartSnapshot.attachMode = (int)part.attachMode;
            protoPartSnapshot.attached = part.isAttached;
            protoPartSnapshot.autostrutMode = part.autoStrutMode;
            protoPartSnapshot.rigidAttachment = part.rigidAttachment;
            protoPartSnapshot.mass = part.mass;
            protoPartSnapshot.shielded = part.ShieldedFromAirstream;
            protoPartSnapshot.temperature = part.temperature;
            protoPartSnapshot.skinTemperature = part.skinTemperature;
            protoPartSnapshot.skinUnexposedTemperature = part.skinUnexposedTemperature;
            protoPartSnapshot.explosionPotential = part.explosionPotential;
            protoPartSnapshot.state = (int)part.State;
            protoPartSnapshot.moduleCosts = part.GetModuleCosts(part.partInfo.cost, ModifierStagingSituation.CURRENT);
            protoPartSnapshot.partRef.onFlightStateSave(protoPartSnapshot.partStateValues);
            protoPartSnapshot.partRef.protoPartSnapshot = protoPartSnapshot;
            protoPartSnapshot.protoModuleCrew = new List<ProtoCrewMember>();
            protoPartSnapshot.protoCrewIndicesBackup = new List<int>();
            protoPartSnapshot.protoCrewNames = new List<string>();
            foreach (var item in part.protoModuleCrew)
            {
                protoPartSnapshot.protoModuleCrew.Add(item);
                protoPartSnapshot.protoCrewNames.Add(item.name);
                proto.AddCrew(item);
            }
            foreach (var partModule in part.Modules)
            {
                protoPartSnapshot.modules.Add(new ProtoPartModuleSnapshot(partModule));
            }
            foreach (var resource in part.Resources)
            {
                protoPartSnapshot.resources.Add(new ProtoPartResourceSnapshot(resource));
            }
            protoPartSnapshot.partEvents = new ConfigNode("EVENTS");
            part.Events.OnSave(protoPartSnapshot.partEvents);
            protoPartSnapshot.partActions = new ConfigNode("ACTIONS");
            part.Actions.OnSave(protoPartSnapshot.partActions);
            protoPartSnapshot.partData = new ConfigNode("PARTDATA");
            part.OnSave(protoPartSnapshot.partData);
            protoPartSnapshot.flagURL = part.flagURL;

            //This is the part that bothers the multithreading, 
            //we just get the transform from the dictionary instead of going against the Transform class
            protoPartSnapshot.refTransformName = PartTransformNames.ContainsKey(part) ?
                PartTransformNames[part] : string.Empty;

            //Old code that was replaced....
            //    if (part.GetReferenceTransform() == null)
            //    {
            //        protoPartSnapshot.refTransformName = string.Empty;
            //    }
            //    else
            //    {
            //        protoPartSnapshot.refTransformName = part.GetReferenceTransform().name;
            //    }

            return protoPartSnapshot;
        }
    }
}
