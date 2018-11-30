using LmpClient.Systems.Flag;
using System;
using System.Linq;

namespace LmpClient.Extensions
{
    public static class ProtoVesselExtension
    {
        /// <summary>
        /// Checks the protovessel for errors
        /// </summary>
        public static bool Validate(this ProtoVessel protoVessel)
        {
            if (protoVessel == null)
            {
                LunaLog.LogError("[LMP]: protoVessel is null!");
                return false;
            }

            if (protoVessel.vesselID == Guid.Empty)
            {
                LunaLog.LogError("[LMP]: protoVessel id is null!");
                return false;
            }

            if (protoVessel.situation == Vessel.Situations.FLYING)
            {
                if (protoVessel.orbitSnapShot == null)
                {
                    LunaLog.LogWarning("[LMP]: Skipping flying vessel load - Protovessel does not have an orbit snapshot");
                    return false;
                }
                if (FlightGlobals.Bodies == null || FlightGlobals.Bodies.Count < protoVessel.orbitSnapShot.ReferenceBodyIndex)
                {
                    LunaLog.LogWarning($"[LMP]: Skipping flying vessel load - Could not find celestial body index {protoVessel.orbitSnapShot.ReferenceBodyIndex}");
                    return false;
                }
            }

            //Fix the flags urls in the vessel. The flag have the value as: "Squad/Flags/default"
            foreach (var part in protoVessel.protoPartSnapshots.Where(p => !string.IsNullOrEmpty(p.flagURL)))
            {
                if (!FlagSystem.Singleton.FlagExists(part.flagURL))
                {
                    LunaLog.Log($"[LMP]: Flag '{part.flagURL}' doesn't exist, setting to default!");
                    part.flagURL = "Squad/Flags/default";
                }
            }
            return true;
        }
    }
}
