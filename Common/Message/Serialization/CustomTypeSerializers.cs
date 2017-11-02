using LunaCommon.Groups;
using LunaCommon.Locks;
using LunaCommon.Message.Data.CraftLibrary;
using System.Collections.Generic;
using System.IO;

namespace LunaCommon.Message.Serialization
{
    public static partial class DataSerializer
    {
        private static void WriteBytesFromKeyValuePairStrCraftListInfo(Stream messageData,
            KeyValuePair<string, CraftListInfo> inputData)
        {
            WriteBytesFromString(messageData, inputData.Key);
            WriteBytesFromCraftListInfo(messageData, inputData.Value);
        }

        private static void WriteBytesFromKeyValuePairStrCraftListInfo_Array(Stream messageData,
            KeyValuePair<string, CraftListInfo>[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromKeyValuePairStrCraftListInfo(messageData, element);
        }

        private static void WriteBytesFromGroupArray(Stream messageData, Group[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromGroup(messageData, element);
        }

        private static void WriteBytesFromGroup(Stream messageData, Group inputData)
        {
            PrivSerialize(inputData, messageData, true);
        }

        private static void WriteBytesFromCraftListInfo(Stream messageData, CraftListInfo inputData)
        {
            PrivSerialize(inputData, messageData, true);
        }

        private static void WriteBytesFromLockDefinitionArray(Stream messageData, LockDefinition[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromLockDefinition(messageData, element);
        }

        private static void WriteBytesFromLockDefinition(Stream messageData, LockDefinition value)
        {
            PrivSerialize(value, messageData, true);
        }
    }
}