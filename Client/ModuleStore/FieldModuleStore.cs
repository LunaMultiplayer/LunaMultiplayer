using LunaClient.ModuleStore.Structures;
using LunaClient.Utilities;
using LunaCommon.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LunaClient.ModuleStore
{
    public class FieldModuleStore
    {
        private static readonly string CustomPartSyncFolder = CommonUtil.CombinePaths(Client.KspPath, "GameData", "LunaMultiPlayer", "PartSync");

        /// <summary>
        /// Here we store all the part modules loaded and its fields
        /// </summary>
        public static readonly Dictionary<Type, FieldModuleDefinition> ModuleFieldsDictionary = new Dictionary<Type, FieldModuleDefinition>();

        /// <summary>
        /// Here we store our customized part module fields
        /// </summary>
        public static Dictionary<string, CustomModuleDefinition> CustomizedModuleFieldsBehaviours = new Dictionary<string, CustomModuleDefinition>();

        /// <summary>
        /// Check all part modules that inherit from PartModule. Then it gets all the fields of those classes that have the "ispersistent" as true.
        /// </summary>
        public static void ReadLoadedPartModules()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var partModules = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(PartModule)));
                foreach (var partModule in partModules)
                {
                    var persistentFields = partModule.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                        .Where(f => f.GetCustomAttributes(typeof(KSPField), true).Any(attr => ((KSPField)attr).isPersistant));

                    ModuleFieldsDictionary.Add(partModule, new FieldModuleDefinition(partModule, persistentFields));
                }
            }

            LunaLog.Log($"Loaded {ModuleFieldsDictionary.Keys.Count} modules and a total of {ModuleFieldsDictionary.Values.Count} fields");
        }

        /// <summary>
        /// Reads the PartsBehaviour.xml
        /// </summary>
        public static void ReadCustomizationXml()
        {
            var filePath = CommonUtil.CombinePaths(CustomPartSyncFolder, "PartsBehaviour.xml");
            if (!File.Exists(filePath))
                LunaXmlSerializer.WriteXml(GetDefaultPartsBehaviour(), filePath);

            List<CustomModuleDefinition> moduleValues;
            try
            {
                moduleValues = LunaXmlSerializer.ReadXml<List<CustomModuleDefinition>>(filePath);
                if (moduleValues.Select(m => m.ModuleName).Distinct().Count() != moduleValues.Count)
                {
                    LunaLog.LogError("Duplicate modules found in PartsBehaviour.xml. Loading default values");
                    moduleValues = GetDefaultPartsBehaviour();
                }
                foreach (var moduleVal in moduleValues)
                {
                    if (moduleVal.Fields.Select(m => m.FieldName).Distinct().Count() != moduleVal.Fields.Count)
                    {
                        LunaLog.LogError($"Duplicate fields found in module {moduleVal.ModuleName} of PartsBehaviour.xml. Loading default values");
                        moduleValues = GetDefaultPartsBehaviour();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"Error reading PartsBehaviour.xml. Loading default values. Details {e}");
                moduleValues = GetDefaultPartsBehaviour();
            }

            CustomizedModuleFieldsBehaviours = moduleValues.ToDictionary(m => m.ModuleName, v => v);
        }

        /// <summary>
        /// Gets a default parts behaviour XML
        /// </summary>
        public static List<CustomModuleDefinition> GetDefaultPartsBehaviour()
        {
            return new List<CustomModuleDefinition>
            {
                new CustomModuleDefinition
                {
                    ModuleName = "ModuleDeployablePart",
                    Fields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            FieldName = "currentRotation",
                            IgnoreReceive = false,
                            IgnoreSend = false,
                            IntervalApplyChangesMs = 1000,
                            IntervalCheckChangesMs = 1000
                        }
                    }
                },
                new CustomModuleDefinition
                {
                    ModuleName = "ModuleEngines",
                    Fields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            FieldName = "currentThrottle",
                            IgnoreReceive = false,
                            IgnoreSend = false,
                            IntervalApplyChangesMs = 1000,
                            IntervalCheckChangesMs = 1000
                        }
                    }
                }
            };
        }
    }
}
