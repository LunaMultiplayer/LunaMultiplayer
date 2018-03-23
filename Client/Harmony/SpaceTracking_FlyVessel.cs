using Harmony;
using KSP.UI.Screens;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to fix the double click event in space tracking
    /// If the tracked vessel is controlled and prelaunch we don't allow you to fly it
    /// </summary>
    [HarmonyPatch(typeof(SpaceTracking))]
    [HarmonyPatch("FlyVessel")]
    public class SpaceTracking_FlyVessel
    {
        [HarmonyPrefix]
        private static bool PrefixFlyVessel(SpaceTracking __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            var vessel = __instance.SelectedVessel;
            if (vessel != null && vessel.situation == Vessel.Situations.PRELAUNCH)
            {
                if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                    return false;
            }

            return true;
        }
    }
}
