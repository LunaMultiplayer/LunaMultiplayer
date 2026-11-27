using LmpCommon.Enums;
using LmpCommon.Message.Data.CraftLibrary;
using System;

namespace LmpCommon.CraftLibrary
{
    /// <summary>
    /// Checks that craft and folder names received from the network are plain names before they become paths.
    /// Rules are the same on every OS, because the server and its clients can run different ones.
    /// Folder names are stricter than craft names, because a craft name always gets ".craft" appended.
    /// </summary>
    public static class CraftPathValidator
    {
        private const int MaxFolderNameLength = 128;

        // Leaves room for ".craft.invalid" inside the 255 character file name limit
        private const int MaxCraftNameLength = 200;

        private const string InvalidCharacters = "\"<>|:*?\\/";

        private static readonly string[] ReservedDeviceNames =
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        /// <summary>
        /// Returns true if the name can safely be used as a folder name.
        /// Also rejects "." and "..", and trailing dots or spaces that Windows silently drops.
        /// </summary>
        public static bool IsSafeFolderName(string name)
        {
            if (!IsPlainName(name, MaxFolderNameLength)) return false;

            return !name.EndsWith(".") && !name.EndsWith(" ");
        }

        /// <summary>
        /// Returns true if the name can safely be used as a craft file name once ".craft" is appended.
        /// Trailing dots and spaces are fine here, so names like "Going to Duna..." keep working.
        /// </summary>
        public static bool IsSafeCraftName(string name)
        {
            return IsPlainName(name, MaxCraftNameLength);
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
            return craft != null && IsSafeFolderName(craft.FolderName) && IsSafeCraftName(craft.CraftName) && IsKnownType(craft.CraftType);
        }

        /// <summary>
        /// Returns true if the folder, name and type of a requested craft are all safe.
        /// </summary>
        public static bool IsSafe(CraftBasicInfo craft)
        {
            return craft != null && IsSafeFolderName(craft.FolderName) && IsSafeCraftName(craft.CraftName) && IsKnownType(craft.CraftType);
        }

        private static bool IsPlainName(string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > maxLength) return false;

            foreach (var character in name)
            {
                if (character < ' ' || InvalidCharacters.IndexOf(character) >= 0) return false;
            }

            return !IsReservedDeviceName(name);
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
