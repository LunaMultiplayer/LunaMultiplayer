using System;
using System.Reflection;

namespace LmpCommon
{
    public class LmpVersioning
    {
        private static Version AssemblyVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        public static ushort MajorVersion { get; } = (ushort)AssemblyVersion.Major;
        public static ushort MinorVersion { get; } = (ushort)AssemblyVersion.Minor;
        public static ushort BuildVersion { get; } = (ushort)AssemblyVersion.Build;

        public static Version CurrentVersion { get; } = new Version(AssemblyVersion.ToString(3));

        /// <summary>
        /// Returns true if the version passed as parameter is compatible with your current version
        /// </summary>
        public static bool IsCompatible(Version version)
        {
            return version.Major == MajorVersion && version.Minor == MinorVersion;
        }

        /// <summary>
        /// Returns true if the version passed as parameter is compatible with your current version
        /// </summary>
        public static bool IsCompatible(string versionStr)
        {
            try
            {
                return IsCompatible(new Version(versionStr));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the version passed as parameter is compatible with your current version
        /// </summary>
        public static bool IsCompatible(int major, int minor, int build)
        {
            return IsCompatible(new Version(major, minor, build));
        }
    }
}
