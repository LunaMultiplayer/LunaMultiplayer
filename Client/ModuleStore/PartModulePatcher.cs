using Harmony;
using Harmony.ILCopying;
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
        
        private static string _currentPartModuleName;
        private static readonly List<CodeInstruction> InstructionsBackup = new List<CodeInstruction>();

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
            HarmonyPatcher.HarmonyInstance.Patch(TestModule.ExampleCallMethod, null, null, new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(InitTranspiler))));
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

                            var persistentFields = partModule.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                                .Where(f => f.GetCustomAttributes(typeof(KSPField), true).Any(attr => ((KSPField)attr).isPersistant)).ToArray();

                            if (persistentFields.Any())
                            {
                                if (FieldModuleStore.CustomizedModuleFieldsBehaviours.TryGetValue(_currentPartModuleName, out var definition))
                                {
                                    //Ignore the whole part module if all the persistent fields are ignored
                                    var ignoredFields = definition.Fields.Where(f => f.Ignore).Select(f => f.FieldName);
                                    if (!persistentFields.Select(f => f.Name).Except(ignoredFields).Any())
                                        continue;
                                }

                                foreach (var partModuleMethod in partModule.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                                .Where(m => !m.IsGenericMethod && MethodSetsPersistentField(m)))
                                {
                                    try
                                    {
                                        LunaLog.Log($"Patching method {partModuleMethod.Name} in module {partModule.Name} of assembly {assembly.FullName}");
                                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, null, new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(Transpiler))));
                                    }
                                    catch
                                    {
                                        LunaLog.LogError($"Could not patch method {partModuleMethod.Name} in module {partModule.Name} of assembly {assembly.FullName}");
                                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, null, new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(Restore))));
                                    }
                                }
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
        /// This method takes the IL code of the part module. If it finds that you're changing the value (OpCodes.Ldstr) of a field that has the attribute "KSPField" and that
        /// attribute has the "isPersistant" then we add the Instructions IL opcodes just after it so the event is triggered.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
                    if (FieldIsIgnored(operand)) continue;

                    var attributes = operand.GetCustomAttributes(typeof(KSPField), false).Cast<KSPField>().ToArray();
                    if (attributes.Any() && attributes.First().isPersistant)
                    {
                        for (var j = 0; j < Instructions.Count; j++)
                        {
                            if (Instructions[j].opcode == OpCodes.Ldstr)
                            {
                                //Change the name operand so the proper "field name" is shown
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

        /// <summary>
        /// This method restores a failed method patch
        /// </summary>
        public static IEnumerable<CodeInstruction> Restore(IEnumerable<CodeInstruction> instructions)
        {
            return InstructionsBackup.AsEnumerable();
        }

        private static bool FieldIsIgnored(FieldInfo fieldInfo)
        {
            if (FieldModuleStore.CustomizedModuleFieldsBehaviours.TryGetValue(_currentPartModuleName, out var definition))
            {
                var fieldDef = definition.Fields.FirstOrDefault(f => f.FieldName == fieldInfo.Name);
                if (fieldDef != null)
                {
                    if (fieldDef.Ignore)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the given method has a IL instruction that SETS a persistent field
        /// </summary>
        private static bool MethodSetsPersistentField(MethodBase partModuleMethod)
        {
            var method = DynamicTools.CreateDynamicMethod(partModuleMethod, "read");
            var instructions = MethodBodyReader.GetInstructions(method.GetILGenerator(), partModuleMethod);

            //OpCodes.Stfld is the opcode for SETTING the value of a field
            foreach (var instruction in instructions.Where(i=> i.opcode == OpCodes.Stfld))
            {
                if (!(instruction.operand is FieldInfo operand)) continue;
                if (FieldIsIgnored(operand)) continue;

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
