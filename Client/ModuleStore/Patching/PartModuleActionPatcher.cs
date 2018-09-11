using Harmony;
using LunaClient.Base;
using LunaClient.ModuleStore.Harmony;
using LunaClient.ModuleStore.Structures;
using System;
using System.Linq;

namespace LunaClient.ModuleStore.Patching
{
    /// <summary>
    /// This class intention is to patch part modules methods so if they modify a field that is persistent it triggers an event
    /// </summary>
    public partial class PartModulePatcher
    {
        /// <summary>
        /// Patches the ACTION methods defined in the XML with the transpiler
        /// </summary>
        private static void PatchActions(Type partModule, ModuleDefinition definition)
        {
            foreach (var actionMethod in definition.ActionMethods)
            {
                var partModuleAction = partModule.GetMethod(actionMethod.MethodName, AccessTools.all);
                if (partModuleAction == null)
                {
                    LunaLog.LogError($"Method {actionMethod.MethodName} not found in part module {partModule.Name}");
                    continue;
                }

                if (!partModuleAction.GetCustomAttributes(typeof(KSPAction), true).Any())
                {
                    LunaLog.LogError($"Method {actionMethod.MethodName} is not a valid KSPAction for part module {partModule.Name}");
                    continue;
                }

                HarmonyPatcher.HarmonyInstance.Patch(partModuleAction, null, HarmonyAction.ActionPostfixMethod);
            }
        }
    }
}
