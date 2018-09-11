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
        /// Patches the Event methods defined in the XML with the transpiler
        /// </summary>
        private static void PatchEvents(Type partModule, ModuleDefinition definition)
        {
            foreach (var eventMethod in definition.EventMethods)
            {
                var partModuleEvent = partModule.GetMethod(eventMethod.MethodName, AccessTools.all);
                if (partModuleEvent == null)
                {
                    LunaLog.LogError($"Method {eventMethod.MethodName} not found in part module {partModule.Name}");
                    continue;
                }

                if (!partModuleEvent.GetCustomAttributes(typeof(KSPEvent), true).Any())
                {
                    LunaLog.LogError($"Method {eventMethod.MethodName} is not a valid KSPEvent for part module {partModule.Name}");
                    continue;
                }

                HarmonyPatcher.HarmonyInstance.Patch(partModuleEvent, null, HarmonyEvent.EventPostfixMethod);
            }
        }
    }
}
