using KSP.UI.Screens.Flight;
using LunaClient.VesselUtilities;
using System.Collections.Generic;
using System.Linq;

namespace LunaClient.Extensions
{
    public static class VesselProtoExtension
    {
        private static readonly List<ProtoCrewMember> MembersToAdd = new List<ProtoCrewMember>();
        private static readonly List<string> MembersToRemove = new List<string>();
        
        /// <summary>
        /// This method will take a vessel and update all it's parts and proto based on a protovessel we received
        /// Protovessel --------------> Vessel & ProtoVessel
        /// This way we avoid having to unload and reload a vessel with it's terrible performance
        /// </summary>
        public static void UpdateVesselFromProtoVessel(this Vessel vessel, ProtoVessel protoVessel, bool forceReload, IEnumerable<uint> vesselPartsId = null)
        {
            if (vessel == null || protoVessel == null || vessel.state == Vessel.State.DEAD) return;

            if (vessel.id != protoVessel.vesselID)
            {
                LunaLog.LogError($"Tried to update a vessel id {vessel.id} with a protovessel of vessel id {protoVessel.vesselID}");
                return;
            }

            if (forceReload)
            {
                VesselLoader.ReloadVessel(protoVessel);
                return;
            }

            var vesselProtoPartIds = vesselPartsId ?? protoVessel.protoPartSnapshots.Select(p => p.flightID);

            //If vessel is UNLOADED it won't have parts so we must take them from the proto...
            var vesselPartsIds = vessel.loaded ? vessel.parts.Select(p => p.flightID) : vessel.protoVessel.protoPartSnapshots.Select(p => p.flightID);

            var hasMissingparts = vesselProtoPartIds.Except(vesselPartsIds).Any();
            if (hasMissingparts)
            {
                //Better to reload if has missing parts as creating them dinamically is a PIA
                VesselLoader.ReloadVessel(protoVessel);
                return;
            }

            if (FlightGlobals.ActiveVessel?.id != vessel.id && (UnloadedVesselChangedSituation(vessel, protoVessel) || VesselJustLandedOrSplashed(vessel, protoVessel)))
            {
                //Reload the whole vessel if vessel lands/splashes as otherwise map view puts the vessel next to the other player.
                //Also reload the whole vessel if is not loaded and situation changed....
                VesselLoader.ReloadVessel(protoVessel);
                return;
            }

            var hasCrewChanges = false;

            //Never do vessel.protoVessel = protoVessel; not even if the vessel is not loaded as when it gets loaded the parts are created in the active vessel 
            //and not on the target vessel

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

                AdjustCrewMembersInProtoPart(protoPartToUpdate, partSnapshot);
                protoPartToUpdate.state = partSnapshot.state;

                var part = protoPartToUpdate.partRef;
                if (part != null) //Part can be null if the vessel is unloaded!!
                {
                    //Remove or add crew members in given part and detect if there have been any change
                    hasCrewChanges |= AdjustCrewMembersInPart(part, partSnapshot);
                }
            }

            //Now kill both parts and protoparts that don't exist
            for (var i = 0; i < protoPartsToRemove.Count; i++)
            {
                //Part can be null if the vessel is unloaded.  In this case, no need to kill it as it's already gone from the game.
                if (protoPartsToRemove[i].partRef != null)
                {
                    //if (protoPartsToRemove[i].partRef.FindModuleImplementing<ModuleDecouple>() != null)
                    //    protoPartsToRemove[i].partRef.decouple();
                    //else
                    protoPartsToRemove[i].partRef.Die();
                }

                vessel.protoVessel.protoPartSnapshots.Remove(protoPartsToRemove[i]);
            }

            if (hasCrewChanges)
            {
                //We must always refresh the crew in every part of the vessel, even if we don't spectate
                vessel.RebuildCrewList();

                //IF we are spectating we must fix the portraits of the kerbals
                if (FlightGlobals.ActiveVessel?.id == vessel.id)
                {
                    //If you don't call spawn crew and you do a crew transfer the transfered crew won't appear in the portraits...
                    MainSystem.Singleton.StartCoroutine(CallbackUtil.DelayedCallback(0.25f, () => { FlightGlobals.ActiveVessel?.SpawnCrew(); }));
                    //If you don't call this the kerbal portraits appear in black...
                    MainSystem.Singleton.StartCoroutine(CallbackUtil.DelayedCallback(0.5f, () => { KerbalPortraitGallery.Instance?.SetActivePortraitsForVessel(FlightGlobals.ActiveVessel); }));
                }
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

        /// <summary>
        /// Add or remove crew from a protopart based on the part snapshot
        /// </summary>
        private static void AdjustCrewMembersInProtoPart(ProtoPartSnapshot protoPartToUpdate, ProtoPartSnapshot partSnapshot)
        {
            if (protoPartToUpdate.protoModuleCrew.Count != partSnapshot.protoModuleCrew.Count)
            {
                MembersToAdd.Clear();
                MembersToRemove.Clear();
                MembersToAdd.AddRange(partSnapshot.protoModuleCrew.Where(mp => protoPartToUpdate.protoModuleCrew.All(m => m.name != mp.name)));
                MembersToRemove.AddRange(protoPartToUpdate.protoModuleCrew.Select(c => c.name).Except(partSnapshot.protoModuleCrew.Select(c => c.name)));

                foreach (var memberToAdd in MembersToAdd)
                {
                    protoPartToUpdate.protoModuleCrew.Add(memberToAdd);
                }

                foreach (var memberToRemove in MembersToRemove)
                {
                    var member = protoPartToUpdate.protoModuleCrew.First(c => c.name == memberToRemove);
                    protoPartToUpdate.protoModuleCrew.Remove(member);
                }
            }
        }

        private static bool VesselJustLandedOrSplashed(Vessel vessel, ProtoVessel protoVessel)
        {
            return !vessel.isEVA && (!vessel.Landed && protoVessel.landed || !vessel.Splashed && protoVessel.splashed);
        }

        private static bool UnloadedVesselChangedSituation(Vessel vessel, ProtoVessel protoVessel)
        {
            return !vessel.loaded && vessel.situation != protoVessel.situation;
        }
    }
}
