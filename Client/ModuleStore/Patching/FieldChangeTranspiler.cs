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
                if (field == null) continue;

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
        ///     String backupField1 = TrackedField1;
        ///     Int backupField2 = TrackedField2;
        ///     Bool backupField3 = TrackedField3;
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
            var fields = _definition.Fields.ToList();
            for (var i = 0; i < fields.Count; i++)
            {
                var field = AccessTools.Field(_originalMethod.DeclaringType, fields[i].FieldName);
                if (field == null) continue;
                //Now all the function method is run...

                //Then we write the comparision and the triggering of the event at the bottom
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
                    //Store the instruction we just added as we must redirect all the returns that the method already had towards this line
                    var firstCheck = _generator.DefineLabel();
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
                    codes[codes.Count - 1].labels.Add(_generator.DefineLabel());
                }

                jmpInstruction.operand = codes[codes.Count - 1].labels[0];
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
