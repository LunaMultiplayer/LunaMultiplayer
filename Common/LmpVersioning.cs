using System;
using System.Reflection;

namespace LunaCommon
{
    public class LmpVersioning
    {
        private static Version AssemblyVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        public static ushort MajorVersion { get; } = (ushort)AssemblyVersion.Major;
        public static ushort MinorVersion { get; } = (ushort)AssemblyVersion.Minor;
        public static ushort BuildVersion { get; } = (ushort)AssemblyVersion.Build;

        public static string CurrentVersion { get; } = AssemblyVersion.ToString(3);
    }
}
