using HarmonyLib;
using LmpCommon.Enums;
using ModuleWheels;
// ReSharper disable All

namespace LmpClient.ModuleStore.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the damage in a wheel if the vessel is not ours
    /// </summary>
    [HarmonyPatch(typeof(ModuleWheelDamage))]
    [HarmonyPatch("FixedUpdate")]
    public class ModuleWheelDamage_FixedUpdate
    {
        [HarmonyPrefix]
        private static bool PrefixFixedUpdate(ModuleWheelDamage __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            return __instance.part.crashTolerance != float.PositiveInfinity;
        }
    }
}
