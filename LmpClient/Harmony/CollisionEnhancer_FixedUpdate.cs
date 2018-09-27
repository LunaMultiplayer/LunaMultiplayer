using Harmony;
using LmpCommon.Enums;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the Collision checks on UNPACKED vessels that are not ours
    /// </summary>
    [HarmonyPatch(typeof(CollisionEnhancer))]
    [HarmonyPatch("FixedUpdate")]
    public class CollisionEnhancer_FixedUpdate
    {
        [HarmonyPrefix]
        private static void PrefixFixedUpdate(CollisionEnhancer __instance, bool __state)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            //Save the current bypass value
            __state = CollisionEnhancer.bypass;

            var shouldBypass = __instance.part != null && float.IsPositiveInfinity(__instance.part.crashTolerance);
            if (shouldBypass)
                CollisionEnhancer.bypass = true;
        }

        [HarmonyPostfix]
        private static void PostfixFixedUpdate(CollisionEnhancer __instance, bool __state)
        {
            //Here we restore the bypass value to what it was before
            CollisionEnhancer.bypass = __state;
        }
    }
}
