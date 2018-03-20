using Harmony;
using System.Reflection;

namespace LunaClient.Base
{
    public static class HarmonyPatcher
    {
        public static void Awake()
        {
            var instance = HarmonyInstance.Create("LunaMultiplayer");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
