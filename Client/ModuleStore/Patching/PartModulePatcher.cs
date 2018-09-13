using Harmony;
using Harmony.ILCopying;
using LunaClient.Base;
using LunaClient.Events;
using LunaClient.ModuleStore.Harmony;
using LunaClient.ModuleStore.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Enumerable = UniLinq.Enumerable;

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
        private static readonly HarmonyMethod FieldChangeTranspilerMethod = new HarmonyMethod(typeof(PartModulePatcher).GetMethod(nameof(FieldChangeTranspiler)));

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
                            FieldChangeTranspilerMethod);
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

        #region Magic stuff

        /// <summary>
        /// This is black magic. By using opcodes we store all the fields that we are tracking into local variables at the beginning of the method.
        /// Then, after the method has run, we compare the current values against the stored ones and we trigger the onPartModuleFieldChanged event
        /// </summary>
        public static IEnumerable<CodeInstruction> FieldChangeTranspiler(ILGenerator generator, MethodBase originalMethod, IEnumerable<CodeInstruction> instructions)
        {
            if (originalMethod.DeclaringType == null) return instructions.AsEnumerable();

            InstructionsBackup.Clear();

            var codes = new List<CodeInstruction>(instructions);
            InstructionsBackup.AddRange(codes);

            var evaluationVar = generator.DeclareLocal(typeof(bool));

            var fields = GetCustomizedFieldsChangedByMethod(originalMethod, _customizationModule).ToList();
            for (var i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                //Here we declare a local var to store the OLD value of the field that we are tracking
                var localVar = generator.DeclareLocal(field.FieldType);
                codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(1, new CodeInstruction(OpCodes.Ldfld, field));

                switch (localVar.LocalIndex)
                {
                    case 0:
                        codes.Insert(2, new CodeInstruction(OpCodes.Stloc_0));
                        break;
                    case 1:
                        codes.Insert(2, new CodeInstruction(OpCodes.Stloc_1));
                        break;
                    case 2:
                        codes.Insert(2, new CodeInstruction(OpCodes.Stloc_2));
                        break;
                    case 3:
                        codes.Insert(2, new CodeInstruction(OpCodes.Stloc_3));
                        break;
                    default:
                        codes.Insert(2, new CodeInstruction(OpCodes.Stloc_S, localVar.LocalIndex));
                        break;

                }

                //Now all the function method is run...
                
                //Then we write the comparision and the triggering of the event at the bottom
                switch (localVar.LocalIndex)
                {
                    case 0:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_0));
                        break;
                    case 1:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_1));
                        break;
                    case 2:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_2));
                        break;
                    case 3:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_3));
                        break;
                    default:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_S, localVar.LocalIndex));
                        break;
                }

                //If we are inserting the first field, redirect all the "returns" towards this instruction so we always do the comparison checks
                if (i == 0)
                {
                    //Store the instruction we just added as we must redirect all the returns that the method already had towards this line
                    var firstCheck = generator.DefineLabel();
                    codes[codes.Count - 1].labels.Add(firstCheck);

                    //This is the last "ret" that every function has
                    var lastReturnInstructionLabel = Enumerable.FirstOrDefault(codes.Last().labels);
                    foreach (var codeInstruction in codes)
                    {
                        if (codeInstruction.operand is Label lbl && lbl == lastReturnInstructionLabel)
                        {
                            codeInstruction.operand = firstCheck;
                        }
                    }
                }

                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldfld, field));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ceq));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldc_I4_0));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ceq));

                switch (evaluationVar.LocalIndex)
                {
                    case 0:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Stloc_0));
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_0));
                        break;
                    case 1:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Stloc_1));
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_1));
                        break;
                    case 2:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Stloc_2));
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_2));
                        break;
                    case 3:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Stloc_3));
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_3));
                        break;
                    default:
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Stloc_S, evaluationVar.LocalIndex));
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_S, evaluationVar.LocalIndex));
                        break;
                }

                var jmpInstruction = new CodeInstruction(OpCodes.Brfalse_S);
                codes.Insert(codes.Count - 1, jmpInstruction);

                LoadFunctionByFieldType(field.FieldType, codes);

                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldstr, field.Name));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldfld, field));

                CallFunctionByFieldType(field.FieldType, codes);

                if (!codes[codes.Count - 1].labels.Any())
                {
                    codes[codes.Count - 1].labels.Add(generator.DefineLabel());
                }

                jmpInstruction.operand = codes[codes.Count - 1].labels[0];
            }

            return codes.AsEnumerable();
        }

        private static void LoadFunctionByFieldType(Type fieldType, List<CodeInstruction> codes)
        {
            if (fieldType.IsEnum)
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleIntFieldChanged")));
            }
            else if (fieldType == typeof(bool))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleBoolFieldChanged")));
            }
            else if (fieldType == typeof(int))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleIntFieldChanged")));
            }
            else if (fieldType == typeof(float))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleFloatFieldChanged")));
            }
            else if (fieldType == typeof(double))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleDoubleFieldChanged")));
            }
            else if (fieldType == typeof(string))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleStringFieldChanged")));
            }
            else if (fieldType == typeof(Quaternion))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleQuaternionFieldChanged")));
            }
            else if (fieldType == typeof(Vector3))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleVectorFieldChanged")));
            }
            else
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleObjectFieldChanged")));
            }
        }

        private static void CallFunctionByFieldType(Type fieldType, List<CodeInstruction> codes)
        {
            if (fieldType.IsEnum)
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, int>), "Fire")));
            }
            else if (fieldType == typeof(bool))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, bool>), "Fire")));
            }
            else if (fieldType == typeof(int))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, int>), "Fire")));
            }
            else if (fieldType == typeof(float))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, float>), "Fire")));
            }
            else if (fieldType == typeof(double))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, double>), "Fire")));
            }
            else if (fieldType == typeof(string))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, string>), "Fire")));
            }
            else if (fieldType == typeof(Quaternion))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, Quaternion>), "Fire")));
            }
            else if (fieldType == typeof(Vector3))
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, Vector3>), "Fire")));
            }
            else
            {
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, object>), "Fire")));
            }
        }

        #endregion

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
