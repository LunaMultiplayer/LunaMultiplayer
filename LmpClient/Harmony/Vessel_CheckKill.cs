using Harmony;
using LmpCommon.Enums;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to avoid run the kill checks in vessels that are immortal
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("CheckKill")]
    public class Vessel_CheckKill
    {
        [HarmonyPrefix]
        private static bool PrefixCheckKill(Vessel __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            //Do not check against the locks as they generate garbage. Instead check if the vessel is immortal by looking at the crash tolerance

            if (__instance.rootPart == null) return true;

            return __instance.rootPart.crashTolerance != float.PositiveInfinity;
        }
    }
}
