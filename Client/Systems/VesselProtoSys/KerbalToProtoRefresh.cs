using System;
using System.Globalization;

namespace LunaClient.Systems.VesselProtoSys
{
    public class KerbalToProtoRefresh
    {
        /// <summary>
        /// Here we refresh the protovessel based on a kerbal. 
        /// Vessel (Kerbal) -----------> Protovessel
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
            else
            {
                for (var i = 0; i < vessel.protoVessel.vesselRef.ActionGroups.groups.Count; i++)
                {
                    var currentVal = vessel.protoVessel.vesselRef.ActionGroups.groups[i];
                    var protoVal = vessel.protoVessel.actionGroups.values[i].value.Substring(0, vessel.protoVessel.actionGroups.values[i].value.IndexOf(",", StringComparison.Ordinal));

                    if (!bool.TryParse(protoVal, out var boolProtoVal) || currentVal != boolProtoVal)
                    {
                        vessel.protoVessel.actionGroups.values[i].value = string.Concat(
                            vessel.protoVessel.vesselRef.ActionGroups.groups[i].ToString(), ", ",
                            vessel.protoVessel.vesselRef.ActionGroups.cooldownTimes[i].ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
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
                return true;
            }

            var partsHaveChanges = false;
            for (var i = 0; i < vessel.parts.Count; i++)
            {
                if (vessel.parts[i].State == PartStates.DEAD) continue;

                if (vessel.parts[i].protoPartSnapshot.state != (int)vessel.parts[i].State)
                {
                    return true;
                }
                
                partsHaveChanges |= RefreshPartModules(vessel.parts[i]);
            }

            return partsHaveChanges;
        }

        /// <summary>
        /// Refreshes all the protovessel part modules based on a part.
        /// Will return true if there's a change in any of the values
        /// </summary>
        private static bool RefreshPartModules(Part part)
        {
            if (part.protoPartSnapshot.modules.Count != part.Modules.Count)
            {
                return true;
            }

            var moduleHaveChanges = false;
            for (var i = 0; i < part.Modules.Count; i++)
            {
                var module = part.Modules[i];
                
                if (module is KerbalEVA kerbalEvaModule)
                {
                    var nodeValue = GetConfigNodeVal("state", module.snapshot.moduleValues);
                    if (kerbalEvaModule.fsm.currentStateName != null && nodeValue == null || string.IsNullOrEmpty(nodeValue?.value))
                        LunaLog.Log($"Detected a fsm change. Node: {nodeValue?.value} current: {kerbalEvaModule.fsm.currentStateName}");
                }

                for (var j = 0; j < module.Fields.Count; j++)
                {
                    var field = module.Fields[j];
                    var nodeValue = GetConfigNodeVal(field.name, module.snapshot.moduleValues);

                    if (nodeValue?.value != null)
                    {
                        var fieldValueAsString = field.GetStringValue(field.host, false);
                        if (fieldValueAsString != null && nodeValue.value != fieldValueAsString)
                        {
                            LunaLog.Log($"Detected a part module change. Module: {module.moduleName} field: {field.name}");
                            moduleHaveChanges = true;
                        }
                    }
                }
            }

            return moduleHaveChanges;
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
            else
            {
                for (var i = 0; i < part.protoPartSnapshot.resources.Count; i++)
                {
                    var resourceSnapshot = part.protoPartSnapshot.resources[i];
                    if (resourceSnapshot.resourceRef == null) continue;

                    resourceSnapshot.amount = resourceSnapshot.resourceRef.amount;
                    resourceSnapshot.flowState = resourceSnapshot.resourceRef.flowState;
                    resourceSnapshot.maxAmount = resourceSnapshot.resourceRef.maxAmount;
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
