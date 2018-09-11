using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LunaClient.ModuleStore.Patching
{
    /// <summary>
    /// This class intention is to patch part modules methods so if they modify a field that is persistent it triggers an event
    /// </summary>
    public partial class PartModulePatcher
    {
        private static readonly List<CodeInstruction> InstructionsBackup = new List<CodeInstruction>();

        #region Transpilers references

        private static readonly HarmonyMethod RestoreTranspilerMethod = new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(Restore)));

        #endregion

        /// <summary>
        /// Call this method to scan all the PartModules and patch the methods
        /// </summary>
        public static void Awake()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var partModules = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(PartModule)));
                    foreach (var partModule in partModules)
                    {
                        try
                        {
                            if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(partModule.Name, out var definition))
                            {
                                PatchActions(partModule, definition);
                                PatchEvents(partModule, definition);
                                PatchMethods(partModule, definition);
                            }
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
        
        /// <summary>
        /// This method takes the IL code of the part module and appends the ActionCallInstructions at the end
        /// </summary>
        public static IEnumerable<CodeInstruction> AppendInstructions(MethodBase originalMethod, IEnumerable<CodeInstruction> instructions, List<CodeInstruction> afterPatch)
        {
            InstructionsBackup.Clear();

            var codes = new List<CodeInstruction>(instructions);
            InstructionsBackup.AddRange(codes);

            for (var i = 0; i < afterPatch.Count; i++)
            {
                if (afterPatch[i].opcode == OpCodes.Ldstr)
                {
                    //Change the name operand so the proper "method name" is shown
                    afterPatch[i].operand = originalMethod.Name;
                }
            }

            codes.InsertRange(codes.Count - 1, afterPatch);

            return codes.AsEnumerable();
        }
    }
}
