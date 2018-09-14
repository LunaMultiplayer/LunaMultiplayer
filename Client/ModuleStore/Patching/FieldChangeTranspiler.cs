using Harmony;
using LunaClient.Events;
using LunaClient.ModuleStore.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace LunaClient.ModuleStore.Patching
{
    public class FieldChangeTranspiler
    {
        private static ModuleDefinition _definition;
        private static ILGenerator _generator;
        private static MethodBase _originalMethod;

        //This is a variable to hold all the comparison "!=" between values
        private static LocalBuilder _evaluationVar;

        //Here we store the local variables index against the field index we are playing with
        private static readonly Dictionary<int, int> FieldIndexToLocalVarDictionary = new Dictionary<int, int>();

        /// <summary>
        /// This is black magic. By using opcodes we store all the fields that we are tracking into local variables at the beginning of the method.
        /// Then, after the method has run, we compare the current values against the stored ones and we trigger the onPartModuleFieldChanged event
        /// </summary>
        public static IEnumerable<CodeInstruction> TranspileMethod(ModuleDefinition definition, ILGenerator generator,
            MethodBase originalMethod, List<CodeInstruction> codes)
        {
            _definition = definition;
            _generator = generator;
            _originalMethod = originalMethod;

            FieldIndexToLocalVarDictionary.Clear();

            _evaluationVar = _generator.DeclareLocal(typeof(bool));
            
            TranspileBackupFields(codes);
            TranspileEvaluations(codes);

            return codes.AsEnumerable();
        }

        /// <summary>
        /// Here we make a backup of all the tracked fields at the BEGGINING of the function.
        /// Ex: 
        /// public void MyFunction()
        /// {
        ///     String backupField1 = TrackedField1;
        ///     Int backupField2 = TrackedField2;
        ///     Bool backupField3 = TrackedField3;
        /// 
        ///     ----- Function Code------
        /// } 
        /// </summary>
        private static void TranspileBackupFields(List<CodeInstruction> codes)
        {
            var fields = _definition.Fields.ToList();
            for (var i = 0; i < fields.Count; i++)
            {
                var field = AccessTools.Field(_originalMethod.DeclaringType, fields[i].FieldName);
                if (field == null)
                {
                    LunaLog.LogError($"Field {fields[i].FieldName} not found in module {_originalMethod.DeclaringType}");
                    continue;
                }

                //Here we declare a local var to store the OLD value of the field that we are tracking
                var localVar = _generator.DeclareLocal(field.FieldType);
                FieldIndexToLocalVarDictionary.Add(i, localVar.LocalIndex);

                codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(1, new CodeInstruction(OpCodes.Ldfld, field));

                switch (FieldIndexToLocalVarDictionary[i])
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
                        codes.Insert(2, new CodeInstruction(OpCodes.Stloc_S, FieldIndexToLocalVarDictionary[i]));
                        break;
                }
            }
        }

        /// Here we make a comparison and we trigger a field when the new vals are different.
        /// Example: 
        /// public void MyFunction()
        /// {
        ///     ----- Fields declared before ----
        /// 
        ///     ----- Function Code------
        /// 
        ///     if (TrackedField1 != backupField1)
        ///         PartModuleEvent.onPartModuleStringFieldChanged.Fire();
        ///     if (TrackedField2 != backupField2)
        ///         PartModuleEvent.onPartModuleIntFieldChanged.Fire();
        ///     if (TrackedField3 != backupField3)
        ///         PartModuleEvent.onPartModuleBoolFieldChanged.Fire();
        /// } 
        private static void TranspileEvaluations(List<CodeInstruction> codes)
        {
            var startComparisonInstructions = new List<CodeInstruction>();
            var jmpInstructions = new List<CodeInstruction>();

            var fields = _definition.Fields.ToList();
            for (var i = 0; i < fields.Count; i++)
            {
                var field = AccessTools.Field(_originalMethod.DeclaringType, fields[i].FieldName);
                if (field == null) continue;

                //We write the comparision and the triggering of the event at the bottom
                switch (FieldIndexToLocalVarDictionary[i])
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
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_S, FieldIndexToLocalVarDictionary[i]));
                        break;
                }

                //If we are inserting the first field, redirect all the "returns" towards this instruction so we always do the comparison checks
                if (i == 0)
                {
                    RedirectExistingReturns(codes);
                }
                else
                {
                    startComparisonInstructions.Add(codes[codes.Count - 2]);
                }

                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldfld, field));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ceq));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldc_I4_0));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ceq));

                switch (_evaluationVar.LocalIndex)
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
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Stloc_S, _evaluationVar.LocalIndex));
                        codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldloc_S, _evaluationVar.LocalIndex));
                        break;
                }
                
                //If we are in the last field then return to the last "ret" of the function
                if (i == fields.Count - 1)
                {
                    if (!codes[codes.Count - 1].labels.Any())
                    {
                        codes[codes.Count - 1].labels.Add(_generator.DefineLabel());
                    }
                    codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Brfalse_S, codes[codes.Count - 1].labels[0]));
                }
                else
                {
                    var jmpInstruction = new CodeInstruction(OpCodes.Brfalse_S);
                    codes.Insert(codes.Count - 1, jmpInstruction);
                    jmpInstructions.Add(jmpInstruction);
                }

                LoadFunctionByFieldType(field.FieldType, codes);

                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldstr, field.Name));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(codes.Count - 1, new CodeInstruction(OpCodes.Ldfld, field));

                CallFunctionByFieldType(field.FieldType, codes);
            }

            FixFallbackInstructions(startComparisonInstructions, jmpInstructions);
        }

        /// <summary>
        /// Now we must fix all the instructions when the if's fail
        /// Example:
        /// public void MyFunction()
        /// {
        ///     ----- Fields declared before ----
        /// 
        ///     ----- Function Code------
        /// 
        ///     if (TrackedField1 != backupField1) -----> If it fails it must get a GOTO "if (TrackedField2 != backupField2)"
        ///         PartModuleEvent.onPartModuleStringFieldChanged.Fire();
        /// 
        ///     if (TrackedField2 != backupField2) -----> If it fails it must get a GOTO "if (TrackedField3 != backupField3)"
        ///         PartModuleEvent.onPartModuleIntFieldChanged.Fire();
        /// 
        ///     if (TrackedField3 != backupField3) -----> This one already jumps to the last ret of the function
        ///         PartModuleEvent.onPartModuleBoolFieldChanged.Fire();
        /// } 
        /// </summary>
        private static void FixFallbackInstructions(List<CodeInstruction> startComparisonInstructions, List<CodeInstruction> jmpInstructions)
        {
            for (var i = 0; i < startComparisonInstructions.Count; i++)
            {
                var lbl = _generator.DefineLabel();
                startComparisonInstructions[i].labels.Add(lbl);
                jmpInstructions[i].operand = lbl;
            }
        }

        /// <summary>
        /// Here we redirect all the "Returns" that the function might have so they point to the first comparison
        /// Example: 
        /// public void MyFunction()
        /// {
        ///     String backupField1 = TrackedField1;
        ///     Int backupField2 = TrackedField2;
        ///     Bool backupField3 = TrackedField3;
        /// 
        ///     ----- Function Code------
        ///     return; ---------------------> This becomes a kind of "GOTO: if (TrackedField1 != backupField1)"
        ///     ----- Function Code------
        /// 
        ///     if (TrackedField1 != backupField1)
        ///         PartModuleEvent.onPartModuleStringFieldChanged.Fire();
        ///     if (TrackedField2 != backupField2)
        ///         PartModuleEvent.onPartModuleIntFieldChanged.Fire();
        ///     if (TrackedField3 != backupField3)
        ///         PartModuleEvent.onPartModuleBoolFieldChanged.Fire();
        /// } 
        /// </summary>
        private static void RedirectExistingReturns(List<CodeInstruction> codes)
        {
            //Store the instruction we just added as we must redirect all the returns that the method already had towards this line
            var firstCheck = _generator.DefineLabel();
            codes[codes.Count - 2].labels.Add(firstCheck);

            //This is the last "ret" that every function has
            var lastReturnInstructionLabel = codes.Last().labels.FirstOrDefault();
            foreach (var codeInstruction in codes)
            {
                if (codeInstruction.opcode == OpCodes.Ret || codeInstruction.operand is Label lbl && lbl == lastReturnInstructionLabel)
                {
                    codeInstruction.opcode = OpCodes.Br;
                    codeInstruction.operand = firstCheck;
                }
            }
        }

        /// <summary>
        /// Calls the correct onPartModuleXXXFieldChanged based on type
        /// </summary>
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

        /// <summary>
        /// Calls the correct Fire() based on type
        /// </summary>
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
    }
}
