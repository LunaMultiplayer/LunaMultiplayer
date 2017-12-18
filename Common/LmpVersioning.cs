using System.Reflection;

namespace LunaCommon
{
    public class LmpVersioning
    {
        public static ushort MajorVersion { get; } = (ushort)Assembly.GetExecutingAssembly().GetName().Version.Major;
        public static ushort MinorVersion { get; } = (ushort)Assembly.GetExecutingAssembly().GetName().Version.Minor;
        public static ushort BuildVersion { get; } = (ushort)Assembly.GetExecutingAssembly().GetName().Version.Build;

        public static string CurrentVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
    }
}
