using UniLinq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselToProtoRefresh
    {
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
            
            vesselHasChanges |= RefreshParts(vessel);

            return vesselHasChanges;
        }

        /// <summary>
        /// Refreshes protovessel parts, protovessel parts modules and protovessel parts resources.
        /// Will return true if there's a change in the part count or a part module has changed
        /// Will NOT return true if the change is only in a part resource
        /// </summary>
        private static bool RefreshParts(Vessel vessel)
        {
            if (vessel.parts.Count != vessel.protoVessel.protoPartSnapshots.Count) return true;

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
            }

            return partsHaveChanges;
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
