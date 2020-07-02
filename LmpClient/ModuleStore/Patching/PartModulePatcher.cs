using Harmony;
using Harmony.ILCopying;
using LmpClient.Base;
using LmpClient.ModuleStore.Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace LmpClient.ModuleStore.Patching
{
    /// <summary>
    /// This class intention is to patch part modules methods so if they modify a field that is customized in the XML it triggers an event
    /// </summary>
    public class PartModulePatcher
    {
        /// <summary>
        /// Here we store the original method instructions in case we fuck things up
        /// </summary>
        private static readonly ConcurrentDictionary<MethodBase, List<CodeInstruction>> InstructionsBackup = new ConcurrentDictionary<MethodBase, List<CodeInstruction>>();

        #region Transpilers references

        private static readonly HarmonyMethod RestoreTranspilerMethod = new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(Restore)));
        private static readonly HarmonyMethod BackupAndCallTranspilerMethod = new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(BackupAndCallTranspiler)));

        #endregion

        /// <summary>
        /// Patches the methods defined in the XML with the transpiler
        /// </summary>
        public static void PatchFieldsAndMethods(Type partModule)
        {
            //If PartModule does not have any customization skip it
            if (!FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(partModule.Name, out var customizationModule) || !customizationModule.CustomizedFields.Any())
                return;

            foreach (var partModuleMethod in partModule.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(m => m.Name == "OnUpdate" || m.Name == "OnFixedUpdate" || m.Name == "FixedUpdate" || m.Name == "Update" || m.Name == "LateUpdate" ||
                            m.GetCustomAttributes(typeof(KSPAction), false).Any() || m.GetCustomAttributes(typeof(KSPEvent), false).Any(a => ((KSPEvent)a).guiActive)))
            {
                if (partModuleMethod.GetMethodBody() != null)
                {
                    try
                    {
                        LunaLog.Log($"Patching method {partModuleMethod.Name} for field changes in module {partModule.Name} of assembly {partModule.Assembly.GetName().Name}");
                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, null, BackupAndCallTranspilerMethod);
                    }
                    catch (Exception ex)
                    {
                        LunaLog.LogError($"Could not patch method {partModuleMethod.Name} for field changes in module {partModule.Name} " +
                                         $"of assembly {partModule.Assembly.GetName().Name}. Details: {ex}");
                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, null, null, RestoreTranspilerMethod);
                    }
                }
            }
        }

        /// <summary>
        /// This method restores a failed method patch
        /// </summary>
        public static IEnumerable<CodeInstruction> Restore(MethodBase originalMethod)
        {
            InstructionsBackup.TryGetValue(originalMethod, out var backupInstructions);
            return backupInstructions.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> BackupAndCallTranspiler(ILGenerator generator, MethodBase originalMethod, IEnumerable<CodeInstruction> instructions)
        {
            if (originalMethod.DeclaringType == null) return instructions.AsEnumerable();

            var codes = new List<CodeInstruction>(instructions);
            InstructionsBackup.AddOrUpdate(originalMethod, codes, (methodBase, oldCodes) => codes);

            var transpiler = new FieldChangeTranspiler(generator, originalMethod, codes);
            return transpiler.Transpile();
        }

        /// <summary>
        /// Checks if the given method has a IL instruction that SETS (therefore, it changes the value) a customized field
        /// </summary>
        private static IEnumerable<FieldInfo> GetCustomizedFieldsChangedByMethod(MethodBase partModuleMethod, ModuleDefinition definition)
        {
            var listOfFields = new HashSet<FieldInfo>();

            var method = DynamicTools.CreateDynamicMethod(partModuleMethod, "read");
            var instructions = MethodBodyReader.GetInstructions(method.GetILGenerator(), partModuleMethod);

            //OpCodes.Stfld is the opcode for SETTING the value of a field
            foreach (var instruction in instructions.Where(i => i.opcode == OpCodes.Stfld))
            {
                if (!(instruction.operand is FieldInfo operand)) continue;

                if (definition.Fields.Any(f => f.FieldName == operand.Name))
                    listOfFields.Add(operand);
            }

            return listOfFields;
        }
    }
}
