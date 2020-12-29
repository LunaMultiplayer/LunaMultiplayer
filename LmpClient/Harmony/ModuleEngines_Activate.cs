using HarmonyLib;
using LmpClient.Events;
// ReSharper disable All

namespace LmpClient.ModuleStore.Harmony
{
    /// <summary>
    /// This harmony patch is intended to relay the fields of the engine gimballs after activating an engine
    /// </summary>
    [HarmonyPatch(typeof(ModuleEngines))]
    [HarmonyPatch("Activate")]
    public class ModuleEngines_Activate
    {
        [HarmonyPostfix]
        private static void PostfixActivate(ModuleEngines __instance)
        {
            if (!__instance.staged)
            {
                foreach (var partModule in __instance.part.FindModulesImplementing<ModuleGimbal>())
                {
                    PartModuleEvent.onPartModuleBoolFieldChanged.Fire(partModule, "gimbalActive", partModule.gimbalActive);
                }
            }
        }
    }
}
