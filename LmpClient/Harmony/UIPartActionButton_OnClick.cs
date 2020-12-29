using HarmonyLib;
using LmpClient.Events;
using LmpClient.ModuleStore;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when pressing a part action button
    /// </summary>
    [HarmonyPatch(typeof(UIPartActionButton))]
    [HarmonyPatch("OnClick")]
    public class UIPartActionButton_OnClick
    {
        [HarmonyPrefix]
        private static void PrefixOnClick(UIPartActionButton __instance, ref PartModule ___partModule, ref BaseEvent ___evt)
        {
            if (__instance.IsModule && ___partModule != null && ___evt != null)
            {
                if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(___partModule.moduleName, out var moduleCustomization) &&
                    moduleCustomization.CustomizedMethods.ContainsKey(___evt.name))
                {
                    PartModuleEvent.onPartModuleMethodCalling.Fire(___partModule, ___evt.name);
                }
            }
        }
    }
}
