using Harmony;
using LunaClient.Base;
using LunaClient.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LunaClient.ModuleStore
{
    /// <summary>
    /// This class intention is to patch part modules methods so if they modify a field that is persistent it triggers an event
    /// </summary>
    public class PartModulePatcher
    {
        /// <summary>
        /// Here we will have the IL code of the method ExampleCall
        /// </summary>
        private static readonly List<CodeInstruction> Instructions = new List<CodeInstruction>();

        private static readonly MethodInfo InitTranspilerMethod = typeof(PartModulePatcher).GetMethod(nameof(InitTranspiler));
        private static readonly MethodInfo TranspilerMethod = typeof(PartModulePatcher).GetMethod(nameof(Transpiler));

        /// <summary>
        /// This is a test class, we use the method "ExampleCall" to take the IL codes and paste them on the real part module methods
        /// </summary>
        private class TestModule : PartModule
        {
            public static readonly MethodInfo ExampleCallMethod = typeof(TestModule).GetMethod(nameof(ExampleCall), AccessTools.all);
            private void ExampleCall() => PartModuleEvent.onPartModuleFieldChange.Fire(this, "FIELDNAME");
        }

        public static IEnumerable<CodeInstruction> InitTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            //We only take the first 4 instructions as the other ones are the return
            Instructions.AddRange(instructions.Take(4));
            return Instructions;
        }

        /// <summary>
        /// Call this method to scan all the PartModules and patch the methods
        /// </summary>
        public static void Awake()
        {
            HarmonyPatcher.HarmonyInstance.Patch(TestModule.ExampleCallMethod, null, null, new HarmonyMethod(InitTranspilerMethod));
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var partModules = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(PartModule)));
                    foreach (var partModule in partModules)
                    {
                        var persistentFields = partModule.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                            .Where(f => f.GetCustomAttributes(typeof(KSPField), true).Any(attr => ((KSPField)attr).isPersistant)).ToArray();

                        if (persistentFields.Any())
                        {
                            foreach (var partModuleMethod in partModule.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                            {
                                try
                                {
                                    HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, null, new HarmonyMethod(TranspilerMethod));
                                }
                                catch
                                {
                                    LunaLog.LogError($"Could not patch method {partModuleMethod.Name} in module {partModule.Name}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LunaLog.LogError($"Exception loading types from assembly {assembly.FullName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// This method takes the IL code of the part module. If it finds that you're changing the value (OpCodes.Ldstr) of a field that has the attribute "KSPField" and that
        /// attribute has the "isPersistant" then we add the Instructions IL opcodes just after it so the event is triggered.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            var length = codes.Count;
            for (var i = 0; i < length; i++)
            {
                if (codes[i].opcode == OpCodes.Stfld)
                {
                    var operand = codes[i].operand as FieldInfo;
                    var attributes = operand?.GetCustomAttributes(typeof(KSPField), false).Cast<KSPField>().ToArray();
                    if (attributes != null && attributes.Any() && attributes.First().isPersistant)
                    {
                        for (var j = 0; j < Instructions.Count; j++)
                        {
                            if (Instructions[j].opcode == OpCodes.Ldstr)
                            {
                                //hange the name operand so the proper "field name" is shown
                                Instructions[j].operand = operand.Name;
                            }
                            
                            codes.Insert(i + 1 + j, new CodeInstruction(Instructions[j]));
                        }

                        length += Instructions.Count;
                        i += Instructions.Count;
                    }
                }
            }

            return codes.AsEnumerable();
        }
    }
}
