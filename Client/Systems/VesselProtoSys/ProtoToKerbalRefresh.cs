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

                var part = protoPartToUpdate.partRef;
                if (part != null) //Part can be null if the vessel is unloaded!!
                {
                    //Set part "state" field... Important for fairings for example...
                    StateField?.SetValue(part, partSnapshot.state);
                    part.ResumeState = part.State;
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
