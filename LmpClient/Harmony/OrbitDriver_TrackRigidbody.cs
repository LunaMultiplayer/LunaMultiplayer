using Harmony;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to call TrackRigidbody correctly when the vessel is kinematic
    /// If you don't run this patch, then the vessels controlled by other players will have an incorrect obt speed.
    /// This means that when spectating they will shake and when trying to grab a ladder your kerbal will be sent off
    /// </summary>
    [HarmonyPatch(typeof(OrbitDriver))]
    [HarmonyPatch("TrackRigidbody")]
    public class OrbitDriver_TrackRigidbody
    {
        /// <summary>
        /// For the prefix we set the vessel as NON kinematic if it's controlled / updated by another player so the TrackRigidbody works as expected
        /// </summary>
        [HarmonyPrefix]
        private static void PrefixTrackRigidbody(OrbitDriver __instance, bool __state)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            __state = false;

            if (__instance.vessel && __instance.vessel.rootPart && __instance.vessel.rootPart.rb && __instance.vessel.rootPart.rb.isKinematic)
            {
                __instance.vessel.rootPart.rb.isKinematic = false;
                __state = true;
            }
        }

        /// <summary>
        /// After the orbit calculations are done we put the vessel back to kinematic mode if needed
        /// </summary>
        [HarmonyPostfix]
        private static void PostfixTrackRigidbody(OrbitDriver __instance, bool __state)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            if (__instance.vessel && __instance.vessel.rootPart && __instance.vessel.rootPart.rb && !__instance.vessel.rootPart.rb.isKinematic && __state)
            { 
                __instance.vessel.rootPart.rb.isKinematic = true;
            }
        }
    }
}
