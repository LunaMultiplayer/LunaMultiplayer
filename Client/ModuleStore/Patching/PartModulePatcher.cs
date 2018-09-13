using Harmony;
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
    public class PartModulePatcher
    {
        /// <summary>
        /// Here we store the original method instructions in case we fuck things up
        /// </summary>
        private static readonly List<CodeInstruction> InstructionsBackup = new List<CodeInstruction>();

        /// <summary>
        /// Keep a reference to know what customization applies
        /// </summary>
        private static ModuleDefinition _customizationModule;

        #region Transpilers references

        private static readonly HarmonyMethod RestoreTranspilerMethod = new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(Restore)));
        private static readonly HarmonyMethod BackupAndCallTranspilerMethod = new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(BackupAndCallTranspiler)));

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
                            if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(partModule.Name, out _customizationModule))
                            {
                                PatchFieldsAndMethods(partModule);
                            }
                        }
                        catch (Exception ex)
                        {
                            LunaLog.LogError($"Exception patching module {partModule.Name} from assembly {assembly.GetName().Name}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LunaLog.LogError($"Exception loading assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Patches the methods defined in the XML with the transpiler
        /// </summary>
        private static void PatchFieldsAndMethods(Type partModule)
        {
            foreach (var partModuleMethod in partModule.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(m => m.Name == "OnUpdate" || m.Name == "OnFixedUpdate" || m.Name == "FixedUpdate" || m.Name == "Update" || m.Name == "LateUpdate" ||
                            m.GetCustomAttributes(typeof(KSPAction), false).Any() || m.GetCustomAttributes(typeof(KSPEvent), false).Any(a => ((KSPEvent)a).guiActive)))
            {
                if (_customizationModule.Fields.Any())
                {
                    var patchMethodCalls = _customizationModule.Methods.Any(m => m.MethodName == partModuleMethod.Name);
                    try
                    {
                        LunaLog.Log(patchMethodCalls
                            ? $"Patching method {partModuleMethod.Name} for field changes and method call in module {partModule.Name} of assembly {partModule.Assembly.GetName().Name}"
                            : $"Patching method {partModuleMethod.Name} for field changes in module {partModule.Name} of assembly {partModule.Assembly.GetName().Name}");

                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, patchMethodCalls ? HarmonyCustomMethod.MethodPrefixMethod : null, null,
                            BackupAndCallTranspilerMethod);
                    }
                    catch
                    {
                        LunaLog.LogError($"Could not patch method {partModuleMethod.Name} for field changes in module {partModule.Name} of assembly {partModule.Assembly.GetName().Name}");
                        HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, patchMethodCalls ? HarmonyCustomMethod.MethodPrefixMethod : null, null,
                            RestoreTranspilerMethod);
                    }
                }
                else if (_customizationModule.Methods.Any(m => m.MethodName == partModuleMethod.Name))
                {
                    if (partModuleMethod.GetParameters().Any())
                    {
                        LunaLog.LogError($"Method {partModuleMethod.Name} is not valid as it doesn't have a parameterless signature");
                        continue;
                    }

                    LunaLog.Log($"Patching method {partModuleMethod.Name} for method call in module {partModule.Name} of assembly {partModule.Assembly.GetName().Name}");
                    HarmonyPatcher.HarmonyInstance.Patch(partModuleMethod, HarmonyCustomMethod.MethodPrefixMethod);
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
        
        public static IEnumerable<CodeInstruction> BackupAndCallTranspiler(ILGenerator generator, MethodBase originalMethod, IEnumerable<CodeInstruction> instructions)
        {
            if (originalMethod.DeclaringType == null) return instructions.AsEnumerable();

            var codes = new List<CodeInstruction>(instructions);
            InstructionsBackup.Clear();
            InstructionsBackup.AddRange(codes);

            return FieldChangeTranspiler.TranspileMethod(_customizationModule, generator, originalMethod, codes);

        }
    }
}
