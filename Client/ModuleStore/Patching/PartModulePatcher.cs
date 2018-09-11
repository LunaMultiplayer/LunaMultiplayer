using Harmony;
using LunaClient.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaClient.ModuleStore.Patching
{
    /// <summary>
    /// This class intention is to patch part modules methods so if they modify a field that is persistent it triggers an event
    /// </summary>
    public partial class PartModulePatcher
    {
        private static string _currentPartModuleName;
        private static readonly List<CodeInstruction> InstructionsBackup = new List<CodeInstruction>();
        
        /// <summary>
        /// Call this method to scan all the PartModules and patch the methods
        /// </summary>
        public static void Awake()
        {
            HarmonyPatcher.HarmonyInstance.Patch(TestModule.ExampleFieldChangeCallMethod, null, null, new HarmonyMethod(typeof(Patching.PartModulePatcher).GetMethod(nameof(InitFieldChangeCallInstructions))));
            HarmonyPatcher.HarmonyInstance.Patch(TestModule.ExampleMethodCallMethod, null, null, new HarmonyMethod(typeof(Patching.PartModulePatcher).GetMethod(nameof(InitMethodCallInstructions))));
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var partModules = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(PartModule)));
                    foreach (var partModule in partModules)
                    {
                        try
                        {
                            _currentPartModuleName = partModule.Name;

                            PatchPersistentFields(partModule, assembly);
                            PatchPersistentMethods(partModule, assembly);
                        }
                        catch (Exception ex)
                        {
                            LunaLog.LogError($"Exception patching module {partModule.Name} from assembly {assembly.FullName}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LunaLog.LogError($"Exception loading assembly {assembly.FullName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// This method restores a failed method patch
        /// </summary>
        public static IEnumerable<CodeInstruction> Restore(IEnumerable<CodeInstruction> instructions)
        {
            return InstructionsBackup.AsEnumerable();
        }
    }
}
