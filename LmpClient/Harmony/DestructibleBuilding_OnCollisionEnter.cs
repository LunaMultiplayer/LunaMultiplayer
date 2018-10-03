using Harmony;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;
using UnityEngine;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the destruction of a building 
    /// if the vessel that crashes against it is controlled/updated by another player
    /// </summary>
    [HarmonyPatch(typeof(DestructibleBuilding))]
    [HarmonyPatch("OnCollisionEnter")]
    public class DestructibleBuilding_OnCollisionEnter
    {
        [HarmonyPrefix]
        private static bool PrefixOnCollisionEnter(Collision c)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            var crashingVessel = c.gameObject.GetComponentUpwards<Part>()?.vessel;
            if (crashingVessel != null)
            {
                if (crashingVessel.rootPart != null)
                    return !float.IsPositiveInfinity(crashingVessel.rootPart.crashTolerance);

                if (!LockSystem.LockQuery.UpdateLockExists(crashingVessel.id)) return true;
                return LockSystem.LockQuery.UpdateLockBelongsToPlayer(crashingVessel.id, SettingsSystem.CurrentSettings.PlayerName);
            }

            return true;
        }
    }
}
