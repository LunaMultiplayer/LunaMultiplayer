using HarmonyLib;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to override the "FindVesselsLandedAt" that sometimes is called to check if there are vessels in a launch site
    /// We just remove the other controlled vessels from that check and set them correctly
    /// </summary>
    [HarmonyPatch(typeof(ShipConstruction))]
    [HarmonyPatch("AssembleForLaunch")]
    [HarmonyPatch(new[]
    {
        typeof(ShipConstruct), typeof(string), typeof(string), typeof(string), typeof(Game),
        typeof(VesselCrewManifest), typeof(bool), typeof(bool), typeof(bool), typeof(bool),
        typeof(Orbit), typeof(bool), typeof(bool)
    })]
    public class ShipConstruction_AssembleForLaunch
    {
        [HarmonyPrefix]
        private static void PrefixAssembleForLaunch(ShipConstruct ship, string landedAt, string displaylandedAt, string flagURL, Game sceneState, VesselCrewManifest crewManifest,
            bool fromShipAssembly, bool setActiveVessel, bool isLanded, bool preCreate, Orbit orbit, bool orbiting, bool isSplashed)
        {
            if (fromShipAssembly && ship != null)
                VesselAssemblyEvent.onAssemblingVessel.Fire(ship);
        }

        [HarmonyPostfix]
        private static void PostfixAssembleForLaunch(ShipConstruct ship, string landedAt, string displaylandedAt, string flagURL, Game sceneState, VesselCrewManifest crewManifest,
            bool fromShipAssembly, bool setActiveVessel, bool isLanded, bool preCreate, Orbit orbit, bool orbiting, bool isSplashed, Vessel __result)
        {
            if (fromShipAssembly && __result && ship != null)
                VesselAssemblyEvent.onAssembledVessel.Fire(__result, ship);
        }
    }
}
