using Harmony;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to avoid that part joins break and create debris if a vessel of another player crashes into ground
    /// </summary>
    [HarmonyPatch(typeof(PartJoint))]
    [HarmonyPatch("OnJointBreak")]
    public class PartJoint_OnJointBreak
    {
        [HarmonyPrefix]
        private static bool PrefixOnJointBreak(PartJoint __instance, float breakForce)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (__instance?.Host.vessel != null)
            {
                return LockSystem.LockQuery.UpdateLockBelongsToPlayer(__instance.Host.vessel.id, SettingsSystem.CurrentSettings.PlayerName);
            }

            return true;
        }
    }
}
