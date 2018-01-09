using KSP.UI.Screens.Flight;
using LunaClient.Utilities;
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

        /// <summary>
        /// Add here modules that fail when calling "OnAwake" on it's partmodule or are useless and can be skipped
        /// </summary>
        public static readonly string[] ModulesToDontAwake = {
            "ModuleWheelBase", "ModuleWheelSteering", "ModuleWheelSuspension", "ModuleScienceContainer", "KerbalEVA"
        };

        /// <summary>
        /// Add here modules that fail when calling "OnLoad" on it's partmodule or are useless and can be skipped
        /// </summary>
        public static readonly string[] ModulesToDontLoad = {
            "ModuleWheelBase", "ModuleWheelSteering", "ModuleWheelSuspension", "ModuleScienceContainer", "KerbalEVA"
        };

        /// <summary>
        /// Add here modules that fail when calling "OnStart" on it's partmodule or are useless and can be skipped
        /// </summary>
        public static readonly string[] ModulesToDontStart = {
            "ModuleWheelBase", "ModuleWheelSteering", "ModuleWheelSuspension", "ModuleScienceContainer", "KerbalEVA"
        };

        /// <summary>
        /// Add here modules that should be ignored (just for performance)
        /// </summary>
        public static readonly string[] ModulesToIgnore = {
            "CModuleLinkedMesh", "FXModuleAnimateThrottle", "ModuleTripLogger"
        };

        /// <summary>
        /// Add fields of a Module that can be ignored (just for performance)
        /// </summary>
        public static readonly Dictionary<string, string[]> FieldsToIgnore = new Dictionary<string, string[]>()
        {
            ["ModuleEnginesFX"] = new[] { "currentThrottle" },
            ["ModuleWheelSuspension"] = new[] { "suspensionPos", "autoBoost" }
        };

        private static readonly List<ProtoCrewMember> MembersToAdd = new List<ProtoCrewMember>();
        private static readonly List<string> MembersToRemove = new List<string>();

        /// <summary>
        /// This method will take a vessel and update all it's parts based on a protovessel
        /// This way we avoid having to unload and reload a vessel and it's terrible performance
        /// </summary>
        public static void UpdateVesselPartsFromProtoVessel(Vessel vessel, ProtoVessel protoVessel)
        {
            if (vessel == null || protoVessel == null || vessel.state == Vessel.State.DEAD) return;

            if (vessel.id != protoVessel.vesselID)
            {
                LunaLog.LogError($"Tried to update a vessel id {vessel.id} with a protovessel of vessel id {protoVessel.vesselID}");
                return;
            }

            var hasMissingparts = vessel.loaded ? protoVessel.protoPartSnapshots.Any(pp => !vessel.parts.Any(p => p.missionID == pp.missionID && p.craftID == pp.craftID)) :
                protoVessel.protoPartSnapshots.Any(pp => !vessel.protoVessel.protoPartSnapshots.Any(p => p.missionID == pp.missionID && p.craftID == pp.craftID));

            if ((vessel.situation != protoVessel.situation && !VesselCommon.IsSpectating) || hasMissingparts)
            {
                //Better to reload the whole vesse if situation changes as it makes the transition more soft.
                //Better to reload if has missing parts as creating them dinamically is a PIA
                VesselLoader.ReloadVessel(protoVessel);
                return;
            }

            var hasCrewChanges = false;

            //Never do vessel.protoVessel = protoVessel; not even if the vessel is not loaded as when it gets loaded the parts are created in the active vessel and not on the target vessel

            //Run trough all the vessel parts. vessel.parts will be empty if vessel is unloaded.
            //Do not do a foreach as vessel parts can change and will result in a modified collection...
            //ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < vessel.parts.Count; i++)
            {
                var part = vessel.parts[i];
                var partSnapshot = protoVessel.protoPartSnapshots.FirstOrDefault(ps => ps.missionID == part.missionID && ps.craftID == part.craftID);
                if (partSnapshot == null) //Part does not exist in the protovessel definition so kill it
                {
                    part.Die();
                    continue;
                }

                //Remove or add crew members in given part and detect if there have been any change
                hasCrewChanges |= AdjustCrewMembersInPart(part, partSnapshot);

                //Set part "state" field... I don't know if this is really needed...
                StateField?.SetValue(part, partSnapshot.state);

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

                        //Sometimes we get a proto part module value of 17.0001 and the part value is 17.0 so it's useless to reload
                        //a whole part module for such a small change! FormatModuleValue() strips the decimals if the value is a decimal
                        var value = existingField.GetValue(existingField.host).ToString().FormatModuleValue();
                        var newVal = definitionPartModuleFieldVals.First(mf => mf.name == existingField.name).value.FormatModuleValue();

                        //Field value between part module and part DEFINITION module are different!
                        if (value != newVal)
                        {
                            PartModuleFields?.SetValue(module, new BaseFieldList(module));
                            module.Fields.Load(moduleSnapshot.moduleValues);

                            if (!ModulesToDontAwake.Contains(module.moduleName))
                                module.OnAwake();
                            if (!ModulesToDontLoad.Contains(module.moduleName))
                                module.OnLoad(moduleSnapshot.moduleValues);
                            if (!ModulesToDontStart.Contains(module.moduleName))
                                module.OnStart(PartModule.StartState.Flying);
                        }
                    }
                }

                //Run trough the poart DEFINITION resources
                foreach (var resourceSnapshot in partSnapshot.resources)
                {
                    //Get the corresponding resource from the actual PART
                    var resource = part.Resources?.FirstOrDefault(pr => pr.info.name == resourceSnapshot.resourceName);
                    if (resource == null) continue;

                    resource.amount = resourceSnapshot.amount;
                }
            }

            if (hasCrewChanges)
            {
                //We must always refresh the crew in every part of the vessel, even if we don't spectate
                vessel.RebuildCrewList();

                //IF we are spectating we must fix the portraits of the kerbals
                if (FlightGlobals.ActiveVessel?.id == vessel.id)
                {
                    //If you don't call spawn crew and you do a crew transfer the transfered crew won't appear in the portraits...
                    Client.Singleton.StartCoroutine(CallbackUtil.DelayedCallback(0.25f, () => { FlightGlobals.ActiveVessel?.SpawnCrew(); }));
                    //If you don't call this the kerbal portraits appear in black...
                    Client.Singleton.StartCoroutine(CallbackUtil.DelayedCallback(0.5f, () => { KerbalPortraitGallery.Instance?.SetActivePortraitsForVessel(FlightGlobals.ActiveVessel); }));
                }
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

        /// <summary>
        /// Add or remove crew from a part based on the part snapshot
        /// </summary>
        private static bool AdjustCrewMembersInPart(Part part, ProtoPartSnapshot partSnapshot)
        {
            if (part.protoModuleCrew.Count != partSnapshot.protoModuleCrew.Count)
            {
                MembersToAdd.Clear();
                MembersToRemove.Clear();
                MembersToAdd.AddRange(partSnapshot.protoModuleCrew.Where(mp => part.protoModuleCrew.All(m => m.name != mp.name)));
                MembersToRemove.AddRange(part.protoModuleCrew.Select(c => c.name).Except(partSnapshot.protoModuleCrew.Select(c => c.name)));

                foreach (var memberToAdd in MembersToAdd)
                {
                    part.AddCrewmember(memberToAdd);
                }

                foreach (var memberToRemove in MembersToRemove)
                {
                    var member = part.protoModuleCrew.First(c => c.name == memberToRemove);
                    part.RemoveCrewmember(member);
                }

                return true;
            }

            return false;
        }
    }
}
