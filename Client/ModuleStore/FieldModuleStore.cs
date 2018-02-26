using LunaClient.ModuleStore.Structures;
using LunaClient.Properties;
using LunaClient.Utilities;
using LunaCommon.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LunaClient.ModuleStore
{
    /// <summary>
    /// This storage class stores all the fields that have the "ispersistent" as true. And also the customizations to them
    /// When we run trough all the part modules looking for changes we will get the fields to check from this class.
    /// Also we will use the customization values to decide if we apply the change or we send it accordingly
    /// </summary>
    public class FieldModuleStore
    {
        private static readonly CustomFieldDefinition DefaultFieldDefinition = new CustomFieldDefinition
        {
            FieldName = string.Empty,
            Ignore = false,
            Interval = 2500,
        };

        private static readonly string CustomPartSyncFolder = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiPlayer", "PartSync");

        /// <summary>
        /// Here we store all the part modules loaded and its fields that have the "ispersistent" as true.
        /// </summary>
        public static readonly Dictionary<string, FieldModuleDefinition> ModuleFieldsDictionary = new Dictionary<string, FieldModuleDefinition>();

        /// <summary>
        /// Here we store our customized part module fields
        /// </summary>
        public static Dictionary<string, CustomModuleDefinition> CustomizedModuleFieldsBehaviours = new Dictionary<string, CustomModuleDefinition>();

        /// <summary>
        /// Here we store the inheritance chain of the types up to PartModule
        /// For example. 
        /// ModuleEngineFX inherits from ModuleEngine and ModuleEngine inherits from PartModule
        /// So for the value of ModuleEngineFX we get an array containing ModuleEngineFX and ModuleEngine
        /// </summary>
        public static Dictionary<string, string[]> InheritanceTypeChain = new Dictionary<string, string[]>();

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
                    var persistentFields = partModule.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                        .Where(f => f.GetCustomAttributes(typeof(KSPField), true).Any(attr => ((KSPField)attr).isPersistant)).ToArray();

                    if (persistentFields.Any())
                    {
                        ModuleFieldsDictionary.Add(partModule.Name, new FieldModuleDefinition(partModule, persistentFields));
                    }

                    InheritanceTypeChain.Add(partModule.Name, GetInheritChain(partModule));
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
            {
                LunaXmlSerializer.WriteToXmlFile(Resources.PartsBehaviour, filePath);
            }

            List<CustomModuleDefinition> moduleValues;
            try
            {
                moduleValues = LunaXmlSerializer.ReadXmlFromPath<List<CustomModuleDefinition>>(filePath);
                if (moduleValues.Select(m => m.ModuleName).Distinct().Count() != moduleValues.Count)
                {
                    LunaLog.LogError("Duplicate modules found in PartsBehaviour.xml. Loading default values");
                    moduleValues = LoadDefaults();
                }
                foreach (var moduleVal in moduleValues)
                {
                    if (moduleVal.Fields.Select(m => m.FieldName).Distinct().Count() != moduleVal.Fields.Count)
                    {
                        LunaLog.LogError($"Duplicate fields found in module {moduleVal.ModuleName} of PartsBehaviour.xml. Loading default values");
                        moduleValues = LoadDefaults();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"Error reading PartsBehaviour.xml. Loading default values. Details {e}");
                moduleValues = LoadDefaults();
            }

            CustomizedModuleFieldsBehaviours = moduleValues.ToDictionary(m => m.ModuleName, v => v);
        }

        private static List<CustomModuleDefinition> LoadDefaults()
        {
            return LunaXmlSerializer.ReadXmlFromString<List<CustomModuleDefinition>>(Resources.PartsBehaviour);
        }

        /// <summary>
        /// Rwturns the customization for a field. if it doesn't exist, it returns a default value
        /// </summary>
        public static CustomFieldDefinition GetCustomFieldDefinition(string moduleName, string fieldName)
        {
            return CustomizedModuleFieldsBehaviours.TryGetValue(moduleName, out var customization) ?
                customization.Fields.FirstOrDefault(f => f.FieldName == fieldName) ?? DefaultFieldDefinition
                : DefaultFieldDefinition;
        }

        /// <summary>
        /// Gets the inheritance chain of a type up to PartModule.
        /// For example. 
        /// ModuleEngineFX inherits from ModuleEngine and ModuleEngine inherits from PartModule
        /// So for the value of ModuleEngineFX we get an array containing ModuleEngineFX and ModuleEngine
        /// </summary>
        private static string[] GetInheritChain(Type partModuleType)
        {
            var list = new List<string>();
            if (partModuleType != null)
            {
                var current = partModuleType;
                while (current != typeof(PartModule) && current != null)
                {
                    list.Add(current.Name);
                    current = current.BaseType;
                }
            }

            return list.ToArray();
        }
    }
}
