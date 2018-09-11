using Harmony;
using Harmony.ILCopying;
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
        /// Here we will have the IL code of the method <see cref="TestModule.ExampleFieldChangeCall"/>
        /// </summary>
        private static readonly List<CodeInstruction> FieldChangeCallInstructions = new List<CodeInstruction>();

        /// <summary>
        /// This is the init transpiler. It will take the instructions of the ExampleCall method and store them in the
        /// <see cref="FieldChangeCallInstructions"/> list so we can use them later.
        /// </summary>
        public static IEnumerable<CodeInstruction> InitFieldChangeCallInstructions(IEnumerable<CodeInstruction> instructions)
        {
            //We only take the first 4 instructions as the other ones are the return
            FieldChangeCallInstructions.AddRange(instructions.Take(4));
            return FieldChangeCallInstructions;
        }

        /// <summary>
        /// Here we call the Transpiler on the needed methods. The ones that CHANGE a persistent field
        /// </summary>
        private static void PatchPersistentFields(Type partModule, Assembly assembly)
        {
            var persistentFields = partModule.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(f => f.GetCustomAttributes(typeof(KSPField), true).Any(attr => ((KSPField)attr).isPersistant))
                .ToArray();

            //Ignore the default customization if there's an specific one
            if (FieldModuleStore.CustomizedModuleBehaviours.ContainsKey(_currentPartModuleName))
            {
                return;
            }

            if (persistentFields.Any())
            {
                foreach (var partModuleMethod in partModule.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsGenericMethod && MethodSetsPersistentField(m)))
                {
                    try
                    {
                        LunaLog.Log($"Patching method {partModuleMethod.Name} for field changes in module {partModule.Name} of assembly {assembly.FullName}");
                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, null, new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(FieldChangeTranspiler))));
                    }
                    catch
                    {
                        LunaLog.LogError($"Could not patch method {partModuleMethod.Name} for field changes in module {partModule.Name} of assembly {assembly.FullName}");
                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, null, new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(Restore))));
                    }
                }
            }
        }

        /// <summary>
        /// This method takes the IL code of the part module. If it finds that you're changing the value (OpCodes.Ldstr) of a field that has the attribute "KSPField" and that
        /// attribute has the "isPersistant" then we add the FieldChangeCallInstructions IL opcodes just after it so the event is triggered.
        /// </summary>
        public static IEnumerable<CodeInstruction> FieldChangeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            InstructionsBackup.Clear();

            var codes = new List<CodeInstruction>(instructions);
            InstructionsBackup.AddRange(codes);

            var length = codes.Count;
            for (var i = 0; i < length; i++)
            {
                //This is the case when a field value is being set
                if (codes[i].opcode == OpCodes.Stfld)
                {
                    if (!(codes[i].operand is FieldInfo operand)) continue;

                    var attributes = operand.GetCustomAttributes(typeof(KSPField), false).Cast<KSPField>().ToArray();
                    if (attributes.Any() && attributes.First().isPersistant)
                    {
                        for (var j = 0; j < FieldChangeCallInstructions.Count; j++)
                        {
                            if (FieldChangeCallInstructions[j].opcode == OpCodes.Ldstr)
                            {
                                //Change the name operand so the proper "field name" is shown
                                FieldChangeCallInstructions[j].operand = operand.Name;
                            }

                            codes.Insert(i + 1 + j, new CodeInstruction(FieldChangeCallInstructions[j]));
                        }

                        length += FieldChangeCallInstructions.Count;
                        i += FieldChangeCallInstructions.Count;
                    }
                }
            }

            return codes.AsEnumerable();
        }

        /// <summary>
        /// Checks if the given method has a IL instruction that SETS (therefore, it changes the value) a persistent field
        /// </summary>
        private static bool MethodSetsPersistentField(MethodBase partModuleMethod)
        {
            var method = DynamicTools.CreateDynamicMethod(partModuleMethod, "read");
            var instructions = MethodBodyReader.GetInstructions(method.GetILGenerator(), partModuleMethod);

            //OpCodes.Stfld is the opcode for SETTING the value of a field
            foreach (var instruction in instructions.Where(i => i.opcode == OpCodes.Stfld))
            {
                if (!(instruction.operand is FieldInfo operand)) continue;

                var attributes = operand.GetCustomAttributes(typeof(KSPField), false).Cast<KSPField>().ToArray();
                if (attributes.Any() && attributes.First().isPersistant)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
