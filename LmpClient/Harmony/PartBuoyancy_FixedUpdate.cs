using Harmony;
using LmpCommon.Enums;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the PartBuoyancy on vessels that are not ours
    /// </summary>
    [HarmonyPatch(typeof(PartBuoyancy))]
    [HarmonyPatch("FixedUpdate")]
    public class PartBuoyancy_FixedUpdate
    {
        [HarmonyPrefix]
        private static bool PrefixFixedUpdate(Part ___part)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (___part.vessel == FlightGlobals.ActiveVessel) return true;
            
            return ___part.vessel.rootPart == null || !float.IsPositiveInfinity(___part.vessel.rootPart.crashTolerance);
        }
    }
}
