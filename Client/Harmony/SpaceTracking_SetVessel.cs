using Harmony;
using KSP.UI;
using KSP.UI.Screens;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to fix the recover button in space tracking
    /// If the tracked vessel is controlled we lock the recover button
    /// </summary>
    [HarmonyPatch(typeof(SpaceTracking))]
    [HarmonyPatch("SetVessel")]
    public class SpaceTracking_SetVessel
    {
        [HarmonyPostfix]
        private static void PostFixSetVessel(ref SpaceTracking __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            if (__instance.SelectedVessel != null)
            {
                if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(__instance.SelectedVessel.id, SettingsSystem.CurrentSettings.PlayerName))
                {
                    __instance.DeleteButton.Lock();
                    __instance.RecoverButton.Lock();
                }
                else
                {
                    __instance.DeleteButton.Unlock();
                    __instance.RecoverButton.Unlock();
                }
            }
        }
    }
}
