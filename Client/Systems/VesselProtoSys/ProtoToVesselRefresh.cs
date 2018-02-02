using KSP.UI.Screens.Flight;
using LunaClient.Utilities;
using LunaClient.VesselIgnore;
using LunaClient.VesselUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LunaClient.Systems.VesselProtoSys
{
    /// <summary>
    /// Class that updates a vessel based on a protovessel definition
    /// </summary>
    public static class ProtoToVesselRefresh
    {
        private static FieldInfo StateField { get; } = typeof(Part).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo PartModuleFields { get; } = typeof(PartModule).GetField("fields", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly List<ProtoCrewMember> MembersToAdd = new List<ProtoCrewMember>();
        private static readonly List<string> MembersToRemove = new List<string>();

        /// <summary>
        /// This method will take a vessel and update all it's parts based on a protovessel
        /// Protovessel --------------> Vessel
        /// This way we avoid having to unload and reload a vessel and it's terrible performance
        /// </summary>
        public static void UpdateVesselPartsFromProtoVessel(Vessel vessel, ProtoVessel protoVessel, IEnumerable<uint> vesselPartsId = null)
        {
            if (vessel == null || protoVessel == null || vessel.state == Vessel.State.DEAD) return;

            if (vessel.id != protoVessel.vesselID)
            {
                LunaLog.LogError($"Tried to update a vessel id {vessel.id} with a protovessel of vessel id {protoVessel.vesselID}");
                return;
            }

            var vesselProtoPartIds = vesselPartsId ?? protoVessel.protoPartSnapshots.Select(p => p.flightID);
            var vesselPartsIds = vessel.parts.Select(p => p.flightID);

            var hasMissingparts = vesselProtoPartIds.Except(vesselPartsIds).Any();
            if (hasMissingparts || (vessel.situation != protoVessel.situation && !VesselCommon.IsSpectating))
            {
                //Better to reload the whole vessel if situation changes as it makes the transition more soft.
                //Better to reload if has missing parts as creating them dinamically is a PIA
                VesselLoader.ReloadVessel(protoVessel);
                return;
            }

            var hasCrewChanges = false;

            //Never do vessel.protoVessel = protoVessel; not even if the vessel is not loaded as when it gets loaded the parts are created in the active vessel 
            //and not on the target vessel

            //Run trough all the vessel parts. vessel.parts will be empty if vessel is unloaded.
            //Do not do a foreach as vessel parts can change and will result in a modified collection...
            for (var i = 0; i < vessel.parts.Count; i++)
            {
                var part = vessel.parts[i];
                var partSnapshot = VesselCommon.FindProtoPartInProtovessel(protoVessel, part.flightID);
                if (partSnapshot == null) //Part does not exist in the protovessel definition so kill it
                {
                    part.Die();
                    continue;
                }

                //Remove or add crew members in given part and detect if there have been any change
                hasCrewChanges |= AdjustCrewMembersInPart(part, partSnapshot);

                //Set part "state" field... I don't know if this is really needed...
                StateField?.SetValue(part, partSnapshot.state);

                UpdatePartModules(partSnapshot, part);
                UpdateVesselResources(partSnapshot, part);
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
        }

        public static void CreateMissingPartsInCurrentProtoVessel(Vessel vessel, ProtoVessel protoVessel)
        {
            //TODO: This is old code where we created parts dinamically but it's quite buggy. It create parts in the CURRENT vessel so it wont work for other vessels

            //We've run trough all the vessel parts and removed the ones that don't exist in the definition.
            //Now run trough the parts in the definition and add the parts that don't exist in the vessel.
            var partsToInit = new List<ProtoPartSnapshot>();
            foreach (var partSnapshot in protoVessel.protoPartSnapshots)
            {
                if (partSnapshot.FindModule("ModuleDockingNode") != null)
                {
                    //We are in a docking port part so remove it from our own vessel if we have it
                    var vesselPart = VesselCommon.FindPartInVessel(vessel, partSnapshot.flightID);
                    if (vesselPart != null)
                    {
                        vesselPart.Die();
                    }
                }

                //Skip parts that already exists
                if (VesselCommon.FindPartInVessel(vessel, partSnapshot.flightID) != null)
                    continue;

                var newPart = partSnapshot.Load(vessel, false);
                vessel.parts.Add(newPart);
                partsToInit.Add(partSnapshot);
            }

            //Init new parts. This must be done in another loop as otherwise new parts won't have their correct attachment parts.
            foreach (var partSnapshot in partsToInit)
                partSnapshot.Init(vessel);

            vessel.RebuildCrewList();
            Client.Singleton.StartCoroutine(CallbackUtil.DelayedCallback(0.25f, () => { FlightGlobals.ActiveVessel?.SpawnCrew(); }));
            Client.Singleton.StartCoroutine(CallbackUtil.DelayedCallback(0.5f, () => { KerbalPortraitGallery.Instance?.SetActivePortraitsForVessel(FlightGlobals.ActiveVessel); }));
        }

        private static void UpdatePartModules(ProtoPartSnapshot partSnapshot, Part part)
        {
            //Run trough all the part DEFINITION modules
            foreach (var moduleSnapshot in partSnapshot.modules.Where(m => !VesselModulesToIgnore.ModulesToIgnore.Contains(m.moduleName)))
            {
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
