using System.Reflection;
using UniLinq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselToProtoRefresh
    {
        private static readonly FieldInfo VesselCrewField = typeof(Vessel).GetField("crew", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ProtoVesselCrewField = typeof(ProtoVessel).GetField("crew", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo FsmField = typeof(ModuleProceduralFairing).GetField("fsm", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Here we refresh the protovessel based on a vessel. 
        /// Vessel -----------> Protovessel
        /// This method is much better than calling vessel.BackupVessel() as it does not generate garbage.
        /// Also it returns true or false if there are substantial changes in the vessel that require updating the whole definition to the server and clients.
        /// </summary>
        public static bool RefreshVesselProto(Vessel vessel)
        {
            return RefreshVesselProto(vessel, vessel.protoVessel);
        }

        /// <summary>
        /// Here we refresh the protovessel based on a vessel. 
        /// Vessel -----------> Protovessel
        /// This method is much better than calling vessel.BackupVessel() as it does not generate garbage.
        /// Also it returns true or false if there are substantial changes in the vessel that require updating the whole definition to the server and clients.
        /// </summary>
        public static bool RefreshVesselProto(Vessel vessel, ProtoVessel protoVesselToRefresh)
        {
            if (vessel.protoVessel == null)
            {
                vessel.protoVessel = new ProtoVessel(vessel);
                return true;
            }

            var rootPartIndex = GetRootPartIndex(vessel);
            var vesselHasChanges = vessel.Landed && !vessel.protoVessel.landed || vessel.Splashed && !vessel.protoVessel.splashed ||
                vessel.currentStage != vessel.protoVessel.stage || vessel.protoVessel.rootIndex != rootPartIndex ||
                vessel.situation != vessel.protoVessel.situation;

            vessel.protoVessel.vesselRef = vessel;
            vessel.protoVessel.vesselRef.protoVessel = vessel.protoVessel;
            vessel.protoVessel.vesselID = vessel.id;
            
            vessel.protoVessel.autoClean = vessel.AutoClean;
            vessel.protoVessel.autoCleanReason = vessel.AutoCleanReason;
            vessel.protoVessel.wasControllable = vessel.IsControllable;

            vessel.protoVessel.CoM = vessel.localCoM;
            vessel.protoVessel.stage = vessel.currentStage;
            vessel.protoVessel.persistent = vessel.isPersistent;

            vesselHasChanges |= RefreshParts(vessel);

            RefreshActionGroups(vessel);

            return vesselHasChanges;
        }

        /// <summary>
        /// Refresh the action groups of the protovessel based on the values of the vessel.
        /// Changes to action groups do not trigger a change to "vesselHasChanges" as the
        /// action groups are sent using the VesselUpdateSys
        /// </summary>
        private static void RefreshActionGroups(Vessel vessel)
        {
            if (vessel.protoVessel.vesselRef.ActionGroups.groups.Count != vessel.protoVessel.actionGroups.CountValues)
            {
                vessel.protoVessel.actionGroups.ClearData();
                vessel.protoVessel.vesselRef.ActionGroups.Save(vessel.protoVessel.actionGroups);
            }
        }

        /// <summary>
        /// Checks if there's a change in the fairings of a vessel
        /// </summary>
        private static bool RefreshFairings(Part part)
        {
            var fairingModule = part.FindModuleImplementing<ModuleProceduralFairing>();
            if (fairingModule != null)
            {
                if (FsmField?.GetValue(fairingModule) is KerbalFSM fsmVal)
                {
                    var currentState = fsmVal.CurrentState;
                    var protoFsmVal = GetConfigNodeVal("fsm", fairingModule.snapshot.moduleValues);

                    if (protoFsmVal != null && currentState.ToString() != protoFsmVal.value)
                    {
                        //Change fairing status as deployed in the proto module snapshot
                        protoFsmVal.value = currentState.ToString();

                        //Remove all the fairing pieces from the proto module snapshot
                        fairingModule.snapshot.moduleValues.RemoveNodes("XSECTION");

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Refreshes protovessel parts, protovessel parts modules and protovessel parts resources.
        /// Will return true if there's a change in the part count or a part module has changed
        /// Will NOT return true if the change is only in a part resource
        /// </summary>
        private static bool RefreshParts(Vessel vessel)
        {
            if (vessel.parts.Count != vessel.protoVessel.protoPartSnapshots.Count)
            {
                vessel.protoVessel.protoPartSnapshots.Clear();

                foreach (var part in vessel.parts.Where(p => p.State != PartStates.DEAD))
                {
                    vessel.protoVessel.protoPartSnapshots.Add(new ProtoPartSnapshot(part, vessel.protoVessel));
                }

                foreach (var partSnapshot in vessel.protoVessel.protoPartSnapshots)
                {
                    partSnapshot.storePartRefs();
                }

                return true;
            }

            var partsHaveChanges = false;
            for (var i = 0; i < vessel.parts.Count; i++)
            {
                if (vessel.parts[i].State == PartStates.DEAD) continue;

                if (vessel.parts[i].protoPartSnapshot.state != (int)vessel.parts[i].State)
                {
                    vessel.parts[i].protoPartSnapshot.state = (int)vessel.parts[i].State;
                    vessel.parts[i].ResumeState = vessel.parts[i].State;
                    partsHaveChanges = true;
                }

                if (vessel.parts[i].protoModuleCrew.Count != vessel.parts[i].protoPartSnapshot.protoModuleCrew.Count)
                {
                    vessel.parts[i].protoPartSnapshot.protoModuleCrew.Clear();
                    vessel.parts[i].protoPartSnapshot.protoModuleCrew.AddRange(vessel.parts[i].protoModuleCrew);

                    vessel.parts[i].protoPartSnapshot.protoCrewNames.Clear();
                    vessel.parts[i].protoPartSnapshot.protoCrewNames.AddRange(vessel.parts[i].protoModuleCrew.Select(c => c.name));

                    partsHaveChanges = true;
                }

                partsHaveChanges |= RefreshFairings(vessel.parts[i]);
                RefreshPartResources(vessel.parts[i]);
            }

            return partsHaveChanges;
        }

        /// <summary>
        /// Refreshes the protovessel part resource based on a part.
        /// We don't check for changes here as doing so will send too much data (resources change VERY often)
        /// </summary>
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
        }

        /// <summary>
        /// Gets a value from a config node
        /// </summary>
        private static ConfigNode.Value GetConfigNodeVal(string name, ConfigNode node)
        {
            for (var i = 0; i < node.CountValues; i++)
            {
                if (node.values[i].name == name)
                    return node.values[i];
            }

            return null;
        }

        /// <summary>
        /// Get the root part index so it can be refreshed in the protovessel
        /// </summary>
        private static int GetRootPartIndex(Vessel vessel)
        {
            for (var i = 0; i < vessel.parts.Count; i++)
            {
                if (vessel.parts[i] == vessel.rootPart)
                    return i;
            }
            return 0;
        }
    }
}
