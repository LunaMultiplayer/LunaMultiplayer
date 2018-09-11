using Harmony;
using LunaClient.Base;
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
        /// <summary>
        /// Here we will have the IL code of the method <see cref="TestModule.ExampleMethodCall"/>
        /// </summary>
        private static readonly List<CodeInstruction> FieldMethodCallInstructions = new List<CodeInstruction>();

        /// <summary>
        /// This is the init transpiler. It will take the instructions of the ExampleCall method and store them in the
        /// <see cref="FieldChangeCallInstructions"/> list so we can use them later.
        /// </summary>
        public static IEnumerable<CodeInstruction> InitMethodCallInstructions(IEnumerable<CodeInstruction> instructions)
        {
            //We only take the first 4 instructions as the other ones are the return
            FieldMethodCallInstructions.AddRange(instructions.Take(4));
            return FieldMethodCallInstructions;
        }

        private static string _patchingMethod;

        /// <summary>
        /// Here we call the Transpiler on the needed methods. The ones specified in the custom XML
        /// </summary>
        private static void PatchPersistentMethods(Type partModule, Assembly assembly)
        {
            if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(_currentPartModuleName, out var definition))
            {
                if (!definition.SyncMethods.Any())
                    return;

                foreach (var partModuleMethod in partModule.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(m => definition.SyncMethods.Any(me=> me.MethodName == m.Name)))
                {
                    _patchingMethod = partModuleMethod.Name;
                    try
                    {
                        LunaLog.Log($"Patching method {partModuleMethod.Name} for method calls in module {partModule.Name} of assembly {assembly.FullName}");
                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, null, new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(MethodCallTranspiler))));
                    }
                    catch
                    {
                        LunaLog.LogError($"Could not patch method {partModuleMethod.Name} for method calls in module {partModule.Name} of assembly {assembly.FullName}");
                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, null, new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(Restore))));
                    }
                }
            }
        }

        /// <summary>
        /// This method takes the IL code of the part module. If it finds that you're changing the value (OpCodes.Ldstr) of a field that has the attribute "KSPField" and that
        /// attribute has the "isPersistant" then we add the FieldChangeCallInstructions IL opcodes just after it so the event is triggered.
        /// </summary>
        public static IEnumerable<CodeInstruction> MethodCallTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            InstructionsBackup.Clear();

            var codes = new List<CodeInstruction>(instructions);
            InstructionsBackup.AddRange(codes);

            for (var i = 0; i < FieldMethodCallInstructions.Count; i++)
            {
                if (FieldMethodCallInstructions[i].opcode == OpCodes.Ldstr)
                {
                    //Change the name operand so the proper "method name" is shown
                    FieldMethodCallInstructions[i].operand = _patchingMethod;
                }

                codes.Insert(i + InstructionsBackup.Count - 1, new CodeInstruction(FieldMethodCallInstructions[i]));
            }

            return codes.AsEnumerable();
        }
    }
}
