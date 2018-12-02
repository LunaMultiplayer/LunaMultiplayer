using Harmony;
using System.Reflection;

namespace LmpClient.Extensions
{
    public static class OrbitDriverExtension
    {
        private static readonly FieldInfo OrbitDriverReady = typeof(OrbitDriver).GetField("ready", AccessTools.all);

        private static readonly MethodInfo OrbitDriverStart = typeof(OrbitDriver).GetMethod("Start", AccessTools.all);

        public static bool Ready(this OrbitDriver driver)
        {
            return (bool)OrbitDriverReady.GetValue(driver);
        }

        public static void ForceStart(this OrbitDriver driver)
        {
            OrbitDriverStart.Invoke(driver, null);
        }
    }
}
