using HarmonyLib;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// LMP rebuilds game state on connect and does not receive ScenarioNewGameIntro
    /// from server sync. This can leave intro flags false and retrigger first-run
    /// tutorial popups on multiplayer load.
    ///
    /// Force all tutorial completion flags to true and skip stock OnLoad.
    /// </summary>
    [HarmonyPatch(typeof(ScenarioNewGameIntro))]
    [HarmonyPatch("OnLoad")]
    public class ScenarioNewGameIntro_OnLoad
    {
        [HarmonyPrefix]
        private static bool PrefixOnLoad(ScenarioNewGameIntro __instance)
        {
            __instance.kscComplete = true;
            __instance.editorComplete = true;
            __instance.tsComplete = true;
            return false;
        }
    }
}
