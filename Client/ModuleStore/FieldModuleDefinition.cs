using System;
using System.Reflection;

namespace LunaClient.ModuleStore
{
    public class FieldModuleDefinition
    {
        public Type ModuleType { get; }
        public FieldInfo[] PersistentModuleField { get; } = new FieldInfo[0];

        public FieldModuleDefinition(Type moduleType, FieldInfo[] persistentFields)
        {
            ModuleType = moduleType;
            PersistentModuleField = persistentFields;
        }
    }
}
