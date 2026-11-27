using LmpCommon.Enums;
using LmpCommon.Message.Data.CraftLibrary;
using System;

namespace LmpCommon.CraftLibrary
{
    /// <summary>
    /// Checks that craft and folder names received from the network are plain file names before they become paths.
    /// Rules are the same on every OS, because the server and its clients can run different ones.
    /// Rejects separators, traversal, invalid characters, Windows device names, and unknown craft types.
    /// </summary>
    public static class CraftPathValidator
    {
        private const int MaxNameLength = 128;

        private const string InvalidCharacters = "\"<>|:*?\\/";

        private static readonly string[] ReservedDeviceNames =
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        /// <summary>
        /// Returns true if the name can safely be used as one folder or file name.
        /// </summary>
        public static bool IsSafeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength) return false;

            // Windows drops trailing dots and spaces, which would let "name." alias "name" and ".." escape the folder
            if (name.EndsWith(".") || name.EndsWith(" ")) return false;

            foreach (var character in name)
            {
                if (character < ' ' || InvalidCharacters.IndexOf(character) >= 0) return false;
            }

            return !IsReservedDeviceName(name);
        }

        /// <summary>
        /// Returns true for the craft types that have a folder on disk.
        /// </summary>
        public static bool IsKnownType(CraftType craftType)
        {
            switch (craftType)
            {
                case CraftType.Vab:
                case CraftType.Sph:
                case CraftType.Subassembly:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns true if the folder, name and type of an uploaded or downloaded craft are all safe.
        /// </summary>
        public static bool IsSafe(CraftInfo craft)
        {
            return craft != null && IsSafeName(craft.FolderName) && IsSafeName(craft.CraftName) && IsKnownType(craft.CraftType);
        }

        /// <summary>
        /// Returns true if the folder, name and type of a requested craft are all safe.
        /// </summary>
        public static bool IsSafe(CraftBasicInfo craft)
        {
            return craft != null && IsSafeName(craft.FolderName) && IsSafeName(craft.CraftName) && IsKnownType(craft.CraftType);
        }

        // Windows treats "CON", "CON.txt" and "CON.craft" as the same device, so only the part before the first dot counts
        private static bool IsReservedDeviceName(string name)
        {
            var dotIndex = name.IndexOf('.');
            var stem = (dotIndex < 0 ? name : name.Substring(0, dotIndex)).TrimEnd();

            return Array.Exists(ReservedDeviceNames, reserved => string.Equals(reserved, stem, StringComparison.OrdinalIgnoreCase));
        }
    }
}
