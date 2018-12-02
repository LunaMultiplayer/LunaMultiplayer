using Harmony;
using System.Reflection;

namespace LmpClient.Extensions
{
    public static class OrbitDriverExtension
    {
        private static readonly FieldInfo OrbitDriverReady = typeof(OrbitDriver).GetField("ready", AccessTools.all);

        public static bool Ready(this OrbitDriver driver)
        {
            return (bool)OrbitDriverReady.GetValue(driver);
        }
    }
}
