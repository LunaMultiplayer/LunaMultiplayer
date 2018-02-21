using LunaClient.Utilities;
using LunaClient.VesselIgnore;
using LunaClient.VesselUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LunaClient.Systems.VesselProtoSys
{
    class ProtoToKerbalRefresh
    {
        private static FieldInfo StateField { get; } = typeof(Part).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo PartModuleFields { get; } = typeof(PartModule).GetField("fields", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// This method will take a vessel and update all it's parts and proto based on a protovessel we received
        /// Protovessel --------------> Vessel & ProtoVessel
        /// This way we avoid having to unload and reload a vessel with it's terrible performance
        /// </summary>
        public static void UpdateKerbalPartsFromProtoVessel(Vessel vessel, ProtoVessel protoVessel, IEnumerable<uint> vesselPartsId = null)
        {
            if (vessel == null || protoVessel == null || vessel.state == Vessel.State.DEAD) return;

            if (vessel.id != protoVessel.vesselID)
            {
                LunaLog.LogError($"Tried to update a vessel id {vessel.id} with a protovessel of vessel id {protoVessel.vesselID}");
                return;
            }

            var vesselProtoPartIds = vesselPartsId ?? protoVessel.protoPartSnapshots.Select(p => p.flightID);

            //If vessel is UNLOADED it won't have parts so we must take them from the proto...
            var vesselPartsIds = vessel.loaded ? vessel.parts.Select(p => p.flightID) : vessel.protoVessel.protoPartSnapshots.Select(p => p.flightID);

            var hasMissingparts = vesselProtoPartIds.Except(vesselPartsIds).Any();
            if (hasMissingparts || !VesselCommon.IsSpectating && (UnloadedVesselChangedSituation(vessel, protoVessel) || VesselJustLandedOrSplashed(vessel, protoVessel)))
            {
                //Reload the whole vessel if vessel lands/splashes as otherwise map view puts the vessel next to the other player.
                //Also reload the whole vessel if it's a EVA or is not loaded and situation changed....
                //Better to reload if has missing parts as creating them dinamically is a PIA
                VesselLoader.ReloadVessel(protoVessel);
                return;
            }
            
            //Run trough all the vessel parts and protoparts. 
            //Vessel.parts will be empty if vessel is unloaded.
            var protoPartsToRemove = new List<ProtoPartSnapshot>();
            for (var i = 0; i < vessel.protoVessel.protoPartSnapshots.Count; i++)
            {
                var protoPartToUpdate = vessel.protoVessel.protoPartSnapshots[i];
                var partSnapshot = VesselCommon.FindProtoPartInProtovessel(protoVessel, protoPartToUpdate.flightID);
                if (partSnapshot == null) //Part does not exist in the protovessel definition so kill it
                {
                    protoPartsToRemove.Add(protoPartToUpdate);
                    continue;
                }
                
                protoPartToUpdate.state = partSnapshot.state;
                UpdatePartModulesInProtoPart(protoPartToUpdate, partSnapshot);
                UpdateProtoVesselResources(protoPartToUpdate, partSnapshot);

                var part = protoPartToUpdate.partRef;
                if (part != null) //Part can be null if the vessel is unloaded!!
                {
                    //Set part "state" field... Important for fairings for example...
                    StateField?.SetValue(part, partSnapshot.state);
                    part.ResumeState = part.State;

                    UpdatePartModules(partSnapshot, part);
                    UpdateVesselResources(partSnapshot, part);
                }
            }

            //Now kill both parts and protoparts that don't exist
            for (var i = 0; i < protoPartsToRemove.Count; i++)
            {
                //Part can be null if the vessel is unloaded.  In this case, no need to kill it as it's already gone from the game.
                if (protoPartsToRemove[i].partRef != null)
                {
                    if (protoPartsToRemove[i].partRef.FindModuleImplementing<ModuleDecouple>() != null)
                        protoPartsToRemove[i].partRef.decouple();
                    else
                        protoPartsToRemove[i].partRef.Die();
                }

                vessel.protoVessel.protoPartSnapshots.Remove(protoPartsToRemove[i]);
            }
        }
        
        private static void UpdatePartModules(ProtoPartSnapshot partSnapshot, Part part)
        {
            //Run trough all the part DEFINITION modules
            foreach (var moduleSnapshot in partSnapshot.modules.Where(m => !VesselModulesToIgnore.ModulesToIgnore.Contains(m.moduleName)))
            {
                if (moduleSnapshot.moduleName == "KerbalEVA")
                {
                    var currentEva = part.FindModuleImplementing<KerbalEVA>();
                    if (currentEva != null)
                    {
                        var lampValStr = moduleSnapshot.moduleValues.GetValue("lampOn");
                        if (bool.TryParse(lampValStr, out var lampVal) && currentEva.lampOn != lampVal)
                        {
                            currentEva.ToggleLamp();
                        }
                    }

                    continue;
                }

                //Get the corresponding module from the actual PART
                var module = part.Modules.Cast<PartModule>().FirstOrDefault(pm => pm.moduleName == moduleSnapshot.moduleName);
                if (module == null) continue;

                var definitionPartModuleFieldVals = moduleSnapshot.moduleValues.values.Cast<ConfigNode.Value>()
                    .Select(v => new { v.name, v.value }).ToArray();
                var partModuleFieldVals = module.Fields.Cast<BaseField>()
                    .Where(f => definitionPartModuleFieldVals.Any(mf => mf.name == f.name)).ToArray();

                //Run trough the current part Modules
                foreach (var existingField in partModuleFieldVals)
                {
                    if (VesselModulesToIgnore.FieldsToIgnore.TryGetValue(module.moduleName, out var fieldsToIgnoreList) &&
                        fieldsToIgnoreList.Contains(existingField.name))
                        continue;

                    //Sometimes we get a proto part module value of 17.0001 and the part value is 17.0 so it's useless to reload
                    //a whole part module for such a small change! FormatModuleValue() strips the decimals if the value is a decimal
                    var value = existingField.GetValue(existingField.host).ToString().FormatModuleValue();
                    var newVal = definitionPartModuleFieldVals.First(mf => mf.name == existingField.name).value
                        .FormatModuleValue();

                    //Field value between part module and part DEFINITION module are different!
                    if (value != newVal)
                    {
                        PartModuleFields?.SetValue(module, new BaseFieldList(module));
                        module.Fields.Load(moduleSnapshot.moduleValues);

                        if (!VesselModulesToIgnore.ModulesToDontAwake.Contains(module.moduleName))
                            module.OnAwake();
                        if (!VesselModulesToIgnore.ModulesToDontLoad.Contains(module.moduleName))
                            module.OnLoad(moduleSnapshot.moduleValues);
                        if (!VesselModulesToIgnore.ModulesToDontStart.Contains(module.moduleName))
                            module.OnStart(PartModule.StartState.Flying);
                    }
                }
            }
        }
        
        private static void UpdateVesselResources(ProtoPartSnapshot partSnapshot, Part part)
        {
            //Run trough the poart DEFINITION resources
            foreach (var resourceSnapshot in partSnapshot.resources)
            {
                //Get the corresponding resource from the actual PART
                var resource = part.Resources?.FirstOrDefault(pr => pr.resourceName == resourceSnapshot.resourceName);
                if (resource == null) continue;

                resource.amount = resourceSnapshot.amount;
            }
        }

        private static void UpdatePartModulesInProtoPart(ProtoPartSnapshot protoPartToUpdate, ProtoPartSnapshot partSnapshot)
        {
            //Run trough all the part DEFINITION modules
            foreach (var moduleSnapshotDefinition in partSnapshot.modules.Where(m => !VesselModulesToIgnore.ModulesToIgnore.Contains(m.moduleName)))
            {
                //Get the corresponding module from the actual vessel PROTOPART
                var currentModule = protoPartToUpdate.FindModule(moduleSnapshotDefinition.moduleName);
                if (currentModule != null)
                {
                    moduleSnapshotDefinition.moduleValues.CopyTo(currentModule.moduleValues);
                }
            }
        }

        private static void UpdateProtoVesselResources(ProtoPartSnapshot protoPartToUpdate, ProtoPartSnapshot partSnapshot)
        {
            //Run trough the poart DEFINITION resources
            foreach (var resourceSnapshot in partSnapshot.resources)
            {
                //Get the corresponding resource from the actual PART
                var resource = protoPartToUpdate.resources?.FirstOrDefault(pr => pr.resourceName == resourceSnapshot.resourceName);
                if (resource == null) continue;

                resource.amount = resourceSnapshot.amount;
            }
        }

        private static bool VesselJustLandedOrSplashed(Vessel vessel, ProtoVessel protoVessel)
        {
            return !vessel.Landed && protoVessel.landed || !vessel.Splashed && protoVessel.splashed;
        }

        private static bool UnloadedVesselChangedSituation(Vessel vessel, ProtoVessel protoVessel)
        {
            return !vessel.loaded && vessel.situation != protoVessel.situation;
        }
    }
}
