using Harmony;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to call TrackRigidbody when the vessel is kinematic
    /// If you don't run this patch, then the vessels controlled by other players will have an incorrect obt speed.
    /// This means that when spectating they will shake and when trying to grab a ladder your kerbal will be sent off
    /// </summary>
    [HarmonyPatch(typeof(OrbitDriver))]
    [HarmonyPatch("TrackRigidbody")]
    public class OrbitDriver_TrackRigidbody
    {
        [HarmonyPostfix]
        private static void PostFixTrackRigidbody(OrbitDriver __instance, CelestialBody refBody, double fdtOffset, ref double ___updateUT)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;
            if (__instance.updateMode == OrbitDriver.UpdateMode.IDLE) return;
            if (__instance.vessel == null || __instance.vessel.rootPart == null || __instance.vessel.rootPart.rb == null || !__instance.vessel.rootPart.rb.isKinematic) return;

            ___updateUT += fdtOffset;
            __instance.vel = __instance.vessel.velocityD.xzy + __instance.orbit.GetRotFrameVelAtPos(__instance.referenceBody, __instance.pos);

            if (refBody != __instance.referenceBody)
            {
                if (__instance.vessel != null)
                {
                    var vector3d = __instance.vessel.CoMD - refBody.position;
                    __instance.pos = vector3d.xzy;
                }
                var frameVel = __instance;
                frameVel.vel = frameVel.vel + (__instance.referenceBody.GetFrameVel() - refBody.GetFrameVel());
            }

            __instance.lastTrackUT = ___updateUT;
            __instance.orbit.UpdateFromStateVectors(__instance.pos, __instance.vel, refBody, ___updateUT);
            __instance.pos.Swizzle();
            __instance.vel.Swizzle();
        }
    }
}
