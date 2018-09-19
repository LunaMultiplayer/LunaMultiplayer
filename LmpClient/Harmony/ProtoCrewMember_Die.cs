using Harmony;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to stop the reputation loss on kerbal dead.
    /// If a kerbal is "dead" and you don't have the kerbal lock then nothing will execute as you don't have the right to kill this kerbal.
    /// </summary>
    [HarmonyPatch(typeof(ProtoCrewMember))]
    [HarmonyPatch("Die")]
    public class ProtoCrewMember_Die
    {
        [HarmonyPrefix]
        private static bool PrefixDie(ProtoCrewMember __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            var kerbalName = __instance.name;
            return LockSystem.LockQuery.KerbalLockBelongsToPlayer(kerbalName, SettingsSystem.CurrentSettings.PlayerName);
        }
    }
}
