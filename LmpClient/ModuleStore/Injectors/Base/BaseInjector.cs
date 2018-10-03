using System;
using System.Reflection;
using Harmony;
using LmpClient.Base;

namespace LmpClient.ModuleStore.Injectors.Base
{
    public abstract class BaseInjector
    {
        public abstract Assembly Assembly { get; }
        public abstract Type Type { get; }
        public abstract MethodInfo Method { get; }

        public abstract MethodInfo PrefixMethod { get; }
        public abstract MethodInfo PostFixMethod { get; }
        
        public void PatchWithInjector()
        {
            if (Method != null)
            {
                var prefix = PrefixMethod != null ? new HarmonyMethod(PrefixMethod) : null;
                var postfix = PostFixMethod != null ? new HarmonyMethod(PostFixMethod) : null;

                HarmonyPatcher.HarmonyInstance.Patch(Method, prefix, postfix);
            }
        }
    }
}
