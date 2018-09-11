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
    /// <summary>
    /// This storage class stores all the fields that have the "ispersistent" as true. And also the customizations to the part modules
    /// When we run trough all the part modules looking for changes we will get the fields to check from this class.
    /// Also we will use the customization methods to act accordingly
    /// </summary>
    public class FieldModuleStore
    {
        private static readonly string CustomPartSyncFolder = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "PartSync");

        /// <summary>
        /// Here we store all the part modules loaded and its fields that have the "ispersistent" as true to use it on the default part sync behaviour.
        /// </summary>
        public static readonly Dictionary<string, FieldModuleDefinition> ModuleFieldsDictionary = new Dictionary<string, FieldModuleDefinition>();

        /// <summary>
        /// Here we store our customized part modules behaviours
        /// </summary>
        public static Dictionary<string, ModuleDefinition> CustomizedModuleBehaviours = new Dictionary<string, ModuleDefinition>();

        /// <summary>
        /// Here we store the inheritance chain of the types up to PartModule
        /// For example. 
        /// ModuleEngineFX inherits from ModuleEngine and ModuleEngine inherits from PartModule
        /// So for the value of ModuleEngineFX we get an array containing ModuleEngineFX and ModuleEngine
        /// </summary>
        public static Dictionary<string, string[]> InheritanceTypeChain = new Dictionary<string, string[]>();

        /// <summary>
        /// Check all part modules that inherit from PartModule. Then it gets all the fields of those classes that have the "ispersistent" as true.
        /// Basically it fills up the ModuleFieldsDictionary dictionary
        /// </summary>
        public static void ReadLoadedPartModules()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
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
                catch (Exception ex)
                {
                    LunaLog.LogError($"Exception loading types from assembly {assembly.FullName}: {ex.Message}");
                }
            }

            LunaLog.Log($"Loaded {ModuleFieldsDictionary.Keys.Count} modules and a total of {ModuleFieldsDictionary.Values.Count} fields");
        }

        /// <summary>
        /// Reads the module customizations xml.
        /// Basically it fills up the CustomizedModuleBehaviours dictionary
        /// </summary>
        public static void ReadCustomizationXml()
        {
            var moduleValues = new List<ModuleDefinition>();

            foreach (var file in Directory.GetFiles(CustomPartSyncFolder, "*.xml"))
            {
                var moduleDefinition = LunaXmlSerializer.ReadXmlFromPath<ModuleDefinition>(file);
                moduleDefinition.ModuleName = Path.GetFileNameWithoutExtension(file);
                moduleValues.Add(moduleDefinition);
            }

            CustomizedModuleBehaviours = moduleValues.ToDictionary(m => m.ModuleName, v => v);
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
