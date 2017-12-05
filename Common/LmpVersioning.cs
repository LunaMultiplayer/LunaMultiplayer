using System.Reflection;

namespace LunaCommon
{
    public class LmpVersioning
    {
        public static string CurrentVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
    }
}
