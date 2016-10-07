using System.Collections.Generic;
using System.IO;
using LunaCommon.Message.Data.CraftLibrary;

namespace LunaCommon.Message.Serialization
{
    public static partial class DataSerializer
    {
        private static void WriteBytesFromKeyValuePairStrCraftListInfo(Stream messageData,
            KeyValuePair<string, CraftListInfo> inputData)
        {
            WriteBytesFromString(messageData, inputData.Key);
            WriteBytesCraftListInfo(messageData, inputData.Value);
        }

        private static void WriteBytesFromKeyValuePairStrCraftListInfo_Array(Stream messageData,
            KeyValuePair<string, CraftListInfo>[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromKeyValuePairStrCraftListInfo(messageData, element);
        }

        private static void WriteBytesCraftListInfo(Stream messageData, CraftListInfo value)
        {
            PrivSerialize(value, messageData, true);
        }
    }
}