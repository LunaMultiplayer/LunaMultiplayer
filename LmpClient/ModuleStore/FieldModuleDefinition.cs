using System;
using System.Collections.Generic;
using System.Reflection;
using UniLinq;

namespace LmpClient.ModuleStore
{
    public class FieldModuleDefinition
    {
        public Type ModuleType { get; }
        public Dictionary<string, FieldInfo> PersistentModuleField { get; }

        public FieldModuleDefinition(Type moduleType, IEnumerable<FieldInfo> persistentFields)
        {
            ModuleType = moduleType;
            PersistentModuleField = persistentFields.ToDictionary(k => k.Name, k => k);
        }
    }
}
