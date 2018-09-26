using LmpClient.ModuleStore.Structures;
using LmpClient.Utilities;
using LmpCommon.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LmpClient.ModuleStore
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
        /// Here we store our customized part modules behaviours
        /// </summary>
        public static Dictionary<string, ModuleDefinition> CustomizedModuleBehaviours = new Dictionary<string, ModuleDefinition>();
        
        /// <summary>
        /// Here we store all the types that inherit from PartModule including the mod files
        /// </summary>
        private static IEnumerable<Type> PartModuleTypes { get; } = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && t.IsSubclassOf(typeof(PartModule)));

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
                moduleDefinition.Init();

                moduleValues.Add(moduleDefinition);
            }

            CustomizedModuleBehaviours = moduleValues.ToDictionary(m => m.ModuleName, v => v);

            var newChildModulesToAdd = new List<ModuleDefinition>();
            foreach (var value in CustomizedModuleBehaviours.Values)
            {
                var moduleClass = PartModuleTypes.FirstOrDefault(t => t.Name == value.ModuleName);
                if (moduleClass != null)
                {
                    AddParentsCustomizations(value, moduleClass);
                    newChildModulesToAdd.AddRange(GetChildCustomizations(value, moduleClass));
                }
            }

            foreach (var moduleToAdd in newChildModulesToAdd)
            {
                CustomizedModuleBehaviours.Add(moduleToAdd.ModuleName, moduleToAdd);
            }
        }

        /// <summary>
        /// Here we return the UNEXISTING CHILD customizations of this moduleDefinition.
        /// Example: ModuleDeployableAntenna inherits from ModuleDeployablePart.
        /// But we don't have ANY ModuleDeployableAntenna customization XML.
        /// Here we return a NEW the ModuleDeployableAntenna XML with the customizations of ModuleDeployablePart
        /// </summary>
        private static List<ModuleDefinition> GetChildCustomizations(ModuleDefinition moduleDefinition, Type moduleClass)
        {
            var newPartModules = new List<ModuleDefinition>();
            foreach (var partModuleType in PartModuleTypes.Where(t => t.BaseType == moduleClass))
            {
                if (!CustomizedModuleBehaviours.ContainsKey(partModuleType.Name))
                {
                    newPartModules.Add(new ModuleDefinition
                    {
                        ModuleName = partModuleType.Name,
                        Fields = moduleDefinition.Fields,
                        Methods = moduleDefinition.Methods,
                    });
                }
            }

            return newPartModules;
        }

        /// <summary>
        /// Here we add the PARENT customizations to this moduleDefinition.
        /// Example: ModuleDeployableSolarPanel inherits from ModuleDeployablePart.
        /// Here we add the fields and methods of ModuleDeployablePart into the ModuleDeployableSolarPanel customizations
        /// </summary>
        private static void AddParentsCustomizations(ModuleDefinition moduleDefinition, Type moduleClass)
        {
            if (moduleClass.BaseType == null || moduleClass.BaseType == typeof(MonoBehaviour)) return;

            if (CustomizedModuleBehaviours.TryGetValue(moduleClass.BaseType.Name, out var parentModule))
            {
                moduleDefinition.MergeWith(parentModule);
            }

            AddParentsCustomizations(moduleDefinition, moduleClass.BaseType);
        }
    }
}
