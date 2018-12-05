using LmpClient.Systems.Chat;
using LmpClient.Systems.Flag;
using LmpClient.Systems.Mod;
using System;
using System.Linq;

namespace LmpClient.Extensions
{
    public static class ProtoVesselExtension
    {
        /// <summary>
        /// Checks if the protovessel has resources, parts that you don't have or that they are banned
        /// </summary>
        public static bool HasInvalidParts(this ProtoVessel pv, bool verboseErrors)
        {
            foreach (var pps in pv.protoPartSnapshots)
            {
                if (ModSystem.Singleton.ModControl && !ModSystem.Singleton.AllowedParts.Contains(pps.partName))
                {
                    if (verboseErrors)
                    {
                        var msg = $"Protovessel {pv.vesselID} ({pv.vesselName}) contains the BANNED PART '{pps.partName}'. Skipping load.";
                        LunaLog.LogWarning(msg);
                        ChatSystem.Singleton.PmMessageServer(msg);
                    }

                    return true;
                }

                if (pps.partInfo == null)
                {
                    if (verboseErrors)
                    {
                        LunaLog.LogWarning($"Protovessel {pv.vesselID} ({pv.vesselName}) contains the MISSING PART '{pps.partName}'. Skipping load.");
                        LunaScreenMsg.PostScreenMessage($"Cannot load '{pv.vesselName}' - missing part: {pps.partName}", 10f, ScreenMessageStyle.UPPER_CENTER);
                    }

                    return true;
                }

                var missingResource = pps.resources.FirstOrDefault(r => !PartResourceLibrary.Instance.resourceDefinitions.Contains(r.resourceName));
                if (missingResource != null && verboseErrors)
                {
                    var msg = $"Protovessel {pv.vesselID} ({pv.vesselName}) contains the MISSING RESOURCE '{missingResource.resourceName}'.";
                    LunaLog.LogWarning(msg);
                    ChatSystem.Singleton.PmMessageServer(msg);

                    LunaScreenMsg.PostScreenMessage($"Vessel '{pv.vesselName}' contains the modded RESOURCE: {pps.partName}", 10f, ScreenMessageStyle.UPPER_CENTER);
                    //We allow loading of vessels that have missing resources. They will be removed by the player with the lock tough...
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true or false in case the protovessel is an asteroid
        /// </summary>
        public static bool IsAsteroid(this ProtoVessel protoVessel)
        {
            if (protoVessel == null) return false;

            if ((protoVessel.protoPartSnapshots == null || protoVessel.protoPartSnapshots.Count == 0) && protoVessel.vesselName.StartsWith("Ast."))
                return true;

            return protoVessel.protoPartSnapshots?.FirstOrDefault()?.partName == "PotatoRoid";
        }

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
