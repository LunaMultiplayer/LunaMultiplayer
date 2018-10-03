using System;
using System.Linq;
using System.Reflection;
using LmpClient.ModuleStore.Injectors.Base;

namespace LmpClient.ModuleStore.Patching
{
    public class InjectorsPatcher
    {
        /// <summary>
        /// Call this method to scan all the Injectors and patch the methods
        /// </summary>
        public static void Awake()
        {
            var injectors = Assembly.GetExecutingAssembly().GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(BaseInjector)));
            foreach (var injector in injectors)
            {
                try
                {
                    var injectorInstance = (BaseInjector)Activator.CreateInstance(injector);
                    injectorInstance.PatchWithInjector();
                }
                catch (Exception ex)
                {
                    LunaLog.LogError($"Exception patching injector {injector.Name}: {ex.Message}");
                }
            }
        }
    }
}
