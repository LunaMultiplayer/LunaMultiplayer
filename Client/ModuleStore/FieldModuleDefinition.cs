using System;
using System.Collections.Generic;
using System.Reflection;

namespace LunaClient.ModuleStore
{
    public class FieldModuleDefinition
    {
        public Type ModuleType { get; }
        public List<FieldInfo> PersistentModuleField { get; } = new List<FieldInfo>();

        public FieldModuleDefinition(Type moduleType, IEnumerable<FieldInfo> persistentFields)
        {
            ModuleType = moduleType;
            PersistentModuleField.AddRange(persistentFields);
        }
    }
}
