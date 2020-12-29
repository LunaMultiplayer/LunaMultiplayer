using HarmonyLib;
using KSP.UI;
using KSP.UI.Screens;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to fix the recover/terminate buttons in space tracking
    /// If the tracked vessel is controlled we lock the recover/terminate button
    /// </summary>
    [HarmonyPatch(typeof(SpaceTracking))]
    [HarmonyPatch("SetVessel")]
    public class SpaceTracking_SetVessel
    {
        [HarmonyPostfix]
        private static void PostfixSetVessel(SpaceTracking __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            if (__instance.SelectedVessel != null)
            {
                if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(__instance.SelectedVessel.id, SettingsSystem.CurrentSettings.PlayerName))
                {
                    if (__instance.SelectedVessel.situation == Vessel.Situations.PRELAUNCH) __instance.FlyButton.Lock();
                    else __instance.FlyButton.Unlock();

                    __instance.DeleteButton.Lock();
                    __instance.RecoverButton.Lock();

                    return;
                }

                //Check if vessel is landed or splashed on kerbin. Otherwise lock the recover button.
                if (__instance.SelectedVessel.IsRecoverable)
                {
                    __instance.FlyButton.Unlock();
                    __instance.DeleteButton.Unlock();
                    __instance.RecoverButton.Unlock();
                }
                else
                {
                    __instance.FlyButton.Unlock();
                    __instance.DeleteButton.Unlock();
                    __instance.RecoverButton.Lock();
                }
            }
        }
    }
}
