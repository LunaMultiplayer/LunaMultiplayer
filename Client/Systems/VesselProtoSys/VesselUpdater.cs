using LunaClient.VesselUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LunaClient.Systems.VesselProtoSys
{
    /// <summary>
    /// Class that updates a vessel based on a protovessel definition
    /// </summary>
    public static class VesselUpdater
    {
        private static FieldInfo StateField { get; } = typeof(Part).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo PartModuleFields { get; } = typeof(PartModule).GetField("fields", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly List<ProtoPartSnapshot> PartsToInit = new List<ProtoPartSnapshot>();

        /// <summary>
        /// Add here modules that fail when calling "OnStart" on it's partmodule
        /// </summary>
        public static readonly List<string> ModulesToDontInit = new List<string>
        {
            "ModuleWheelBase", "ModuleWheelSteering", "ModuleWheelSuspension"
        };

        /// <summary>
        /// Add here modules that should be ignored (just for performance)
        /// </summary>
        public static readonly List<string> ModulesToIgnore = new List<string>
        {
            "CModuleLinkedMesh", "FXModuleAnimateThrottle"
        };

        /// <summary>
        /// Add fields of a Module that can be ignored (just for performance)
        /// </summary>
        public static readonly Dictionary<string, List<string>> FieldsToIgnore = new Dictionary<string, List<string>>()
        {
            ["ModuleEnginesFX"] = new List<string>{ "currentThrottle" }
        };

        /// <summary>
        /// This method will take a vessel and update all it's parts based on a protovessel
        /// This way we avoid having to unload and reload a vessel and it's terrible performance
        /// </summary>
        public static void UpdateVesselPartsFromProtoVessel(Vessel vessel, ProtoVessel protoVessel)
        {
            if (vessel.id != protoVessel.vesselID)
            {
                LunaLog.LogError($"Tried to update a vessel id {vessel.id} with a protovessel of vessel id {protoVessel.vesselID}");
                return;
            }

            if (vessel.situation != protoVessel.situation)
            {
                //Better to reload the whole vesse if situation changes as it makes the transition more soft.
                VesselLoader.ReloadVessel(protoVessel);
                return;
            }

            //Never do vessel.protoVessel = protoVessel; not even if the vessel is not loaded as when it gets loaded the parts are created in the active vessel and not on the target vessel

            //Run trough all the vessel parts. vessel.parts will be empty if vessel is unloaded
            foreach (var part in vessel.parts)
            {
                var partSnapshot = protoVessel.protoPartSnapshots.FirstOrDefault(ps => ps.missionID == part.missionID && ps.craftID == part.craftID);
                if (partSnapshot == null) //Part does not exist in the protovessel definition so kill it
                {
                    part.Die();
                    continue;
                }

                //Set part "state" field... I don't know if this is really needed...
                StateField.SetValue(part, partSnapshot.state);

                //Run trough all the part DEFINITION modules
                foreach (var moduleSnapshot in partSnapshot.modules.Where(m => !ModulesToIgnore.Contains(m.moduleName)))
                {
                    //Get the corresponding module from the actual PART
                    var module = part.Modules.Cast<PartModule>().FirstOrDefault(pm => pm.moduleName == moduleSnapshot.moduleName);
                    if (module == null) continue;

                    var definitionPartModuleFieldVals = moduleSnapshot.moduleValues.values.Cast<ConfigNode.Value>().Select(v => new { v.name, v.value }).ToArray();
                    var partModuleFieldVals = module.Fields.Cast<BaseField>().Where(f => definitionPartModuleFieldVals.Any(mf => mf.name == f.name)).ToArray();

                    //Run trough the current part Modules
                    foreach (var existingField in partModuleFieldVals)
                    {
                        if (FieldsToIgnore.TryGetValue(module.moduleName, out var fieldsToIgnoreList) && fieldsToIgnoreList.Contains(existingField.name))
                            continue;

                        var value = existingField.GetValue(existingField.host).ToString();
                        var newVal = definitionPartModuleFieldVals.First(mf => mf.name == existingField.name).value;

                        //Field value between part module and part DEFINITION module are different!
                        if (value != newVal)
                        {
                            PartModuleFields.SetValue(module, new BaseFieldList(module));
                            module.Fields.Load(moduleSnapshot.moduleValues);

                            if (!ModulesToDontInit.Contains(module.moduleName))
                                module.OnStart(PartModule.StartState.None);
                        }
                    }
                }

                //Run trough the poart DEFINITION resources
                foreach (var resourceSnapshot in partSnapshot.resources)
                {
                    //Get the corresponding resource from the actual PART
                    var resource = part.Resources.FirstOrDefault(pr => pr.info.name == resourceSnapshot.resourceName);
                    if (resource == null) continue;

                    resource.amount = resourceSnapshot.amount;
                }
            }

            var hasMissingparts = vessel.loaded ? protoVessel.protoPartSnapshots.Any(pp => !vessel.parts.Any(p => p.missionID == pp.missionID && p.craftID == pp.craftID)) :
                protoVessel.protoPartSnapshots.Any(pp => !vessel.protoVessel.protoPartSnapshots.Any(p => p.missionID == pp.missionID && p.craftID == pp.craftID));

            if (hasMissingparts)
            {
                VesselLoader.ReloadVessel(protoVessel);
            }

            //TODO: This is old code where we created parts dinamically but it's quite buggy. It create parts in the CURRENT vessel instead of in the target vessel
            ////We've run trough all the vessel parts and removed the ones that don't exist in the definition.
            ////Now run trough the parts in the definition and add the parts that don't exist in the vessel.
            //PartsToInit.Clear();
            //foreach (var partSnapshot in protoVessel.protoPartSnapshots)
            //{
            //    //Skip parts that already exists
            //    if (vessel.parts.Any(p => p.missionID == partSnapshot.missionID && p.craftID == partSnapshot.craftID))
            //        continue;

            //    var newPart = partSnapshot.Load(vessel, false);
            //    vessel.parts.Add(newPart);
            //    PartsToInit.Add(partSnapshot);
            //}

            ////Init new parts. This must be done in another loop as otherwise new parts won't have their correct attachment parts.
            //foreach (var partSnapshot in PartsToInit)
            //    partSnapshot.Init(vessel);
        }
    }
}
