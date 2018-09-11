using Harmony;
using LunaClient.Base;
using LunaClient.ModuleStore.Harmony;
using LunaClient.ModuleStore.Structures;
using System;
using System.Linq;

namespace LunaClient.ModuleStore.Patching
{
    /// <summary>
    /// This class intention is to patch part modules methods so if they modify a field that is persistent it triggers an Method
    /// </summary>
    public partial class PartModulePatcher
    {
        /// <summary>
        /// Patches the ACTION methods defined in the XML with the transpiler
        /// </summary>
        private static void PatchMethods(Type partModule, ModuleDefinition definition)
        {
            foreach (var method in definition.Methods)
            {
                var partModuleMethod = partModule.GetMethod(method.MethodName, AccessTools.all);
                if (partModuleMethod == null)
                {
                    LunaLog.LogError($"Method {method.MethodName} not found in part module {partModule.Name}");
                    continue;
                }

                if (partModuleMethod.GetCustomAttributes(typeof(KSPEvent), true).Any() || partModuleMethod.GetCustomAttributes(typeof(KSPAction), true).Any())
                {
                    LunaLog.LogError($"Method {method.MethodName} is not valid as it has the KSPMethod/KSPAction attribute");
                    continue;
                }

                if (partModuleMethod.GetParameters().Any())
                {
                    LunaLog.LogError($"Method {method.MethodName} is not valid as it doesn't have a parameterless signature");
                    continue;
                }


                HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, HarmonyCustomMethod.MethodPostfixMethod);
            }
        }
    }
}
