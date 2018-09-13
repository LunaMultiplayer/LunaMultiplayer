using Harmony;
using LunaClient.Events;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LunaClient.ModuleStore.Harmony
{
    public class HarmonyCustomMethod
    {
        public static readonly HarmonyMethod MethodPrefixMethod = new HarmonyMethod(typeof(HarmonyCustomMethod).GetMethod(nameof(MethodPrefix)));

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static void MethodPrefix(PartModule __instance, MethodInfo __originalMethod)
        {
            PartModuleEvent.onPartModuleMethodCalling.Fire(__instance, __originalMethod.Name);
        }
    }
}
