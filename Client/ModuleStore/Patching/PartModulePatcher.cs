using Harmony;
using Harmony.ILCopying;
using LunaClient.Base;
using LunaClient.ModuleStore.Harmony;
using LunaClient.ModuleStore.Structures;
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
        /// Here we store the original method instructions in case we fuck things up
        /// </summary>
        private static readonly List<CodeInstruction> InstructionsBackup = new List<CodeInstruction>();

        /// <summary>
        /// Here we will have the IL code of the method <see cref="TestModule.ExampleFieldChangeCall"/>
        /// </summary>
        private static readonly List<CodeInstruction> FieldChangeCallInstructions = new List<CodeInstruction>();

        #region Transpilers references

        private static readonly HarmonyMethod RestoreTranspilerMethod = new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(Restore)));
        private static readonly HarmonyMethod InitFieldChangeTranspilerMethod = new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(InitFieldChangeCallInstructions)));
        private static readonly HarmonyMethod FieldChangeTranspilerMethod = new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(FieldChangeTranspiler)));
        #endregion

        /// <summary>
        /// Call this method to scan all the PartModules and patch the methods
        /// </summary>
        public static void Awake()
        {
            HarmonyPatcher.HarmonyInstance.Patch(TestModule.ExampleFieldChangeCallMethod, null, null, InitFieldChangeTranspilerMethod);
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
                                PatchFieldsAndMethods(partModule, definition);
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
        /// Patches the ACTION methods defined in the XML with the transpiler
        /// </summary>
        private static void PatchFieldsAndMethods(Type partModule, ModuleDefinition definition)
        {
            foreach (var partModuleMethod in partModule.GetMethods(AccessTools.all))
            {
                if (definition.Fields.Any() && MethodSetsCustomizedField(partModuleMethod, definition))
                {
                    var patchMethodCalls = definition.Methods.Any(m => m.MethodName == partModuleMethod.Name);
                    try
                    {
                        LunaLog.Log(patchMethodCalls
                            ? $"Patching method {partModuleMethod.Name} for field changes and method call in module {partModule.Name} of assembly {partModule.Assembly.FullName}"
                            : $"Patching method {partModuleMethod.Name} for field changes in module {partModule.Name} of assembly {partModule.Assembly.FullName}");

                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, patchMethodCalls ? HarmonyCustomMethod.MethodPostfixMethod : null,
                            FieldChangeTranspilerMethod);
                    }
                    catch
                    {
                        LunaLog.LogError($"Could not patch method {partModuleMethod.Name} for field changes in module {partModule.Name} of assembly {partModule.Assembly.FullName}");
                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, patchMethodCalls ? HarmonyCustomMethod.MethodPostfixMethod : null, 
                            RestoreTranspilerMethod);
                    }
                }
                else if (definition.Methods.Any(m => m.MethodName == partModuleMethod.Name))
                {
                    if (partModuleMethod.GetParameters().Any())
                    {
                        LunaLog.LogError($"Method {partModuleMethod.Name} is not valid as it doesn't have a parameterless signature");
                        continue;
                    }

                    LunaLog.Log($"Patching method {partModuleMethod.Name} for method call in module {partModule.Name} of assembly {partModule.Assembly.FullName}");
                    HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, HarmonyCustomMethod.MethodPostfixMethod);
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
        /// Checks if the given method has a IL instruction that SETS (therefore, it changes the value) a customized field
        /// </summary>
        private static bool MethodSetsCustomizedField(MethodBase partModuleMethod, ModuleDefinition definition)
        {
            var method = DynamicTools.CreateDynamicMethod(partModuleMethod, "read");
            var instructions = MethodBodyReader.GetInstructions(method.GetILGenerator(), partModuleMethod);

            //OpCodes.Stfld is the opcode for SETTING the value of a field
            foreach (var instruction in instructions.Where(i => i.opcode == OpCodes.Stfld))
            {
                if (!(instruction.operand is FieldInfo operand)) continue;

                if (definition.Fields.Any(f => f.FieldName == operand.Name))
                    return true;
            }

            return false;
        }
    }
}
