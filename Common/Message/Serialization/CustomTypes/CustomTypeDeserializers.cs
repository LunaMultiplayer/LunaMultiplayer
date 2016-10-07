using System.Collections.Generic;
using System.IO;
using LunaCommon.Message.Data.CraftLibrary;

namespace LunaCommon.Message.Serialization
{
    public static partial class DataDeserializer
    {
        private static CraftListInfo GetCraftListInfoFromBytes(Stream messageData)
        {
            var craftInfo = new CraftListInfo();
            PrivDeserialize(messageData, craftInfo);

            return craftInfo;
        }

        private static KeyValuePair<string, CraftListInfo> GetKeyValuePairStrCraftListInfo_FromBytes(Stream messageData)
        {
            var outputData = GetByteArrayFromBytes(messageData);
            var keyString = BaseSerializer.Encoder.GetString(outputData);

            var craftInfo = GetCraftListInfoFromBytes(messageData);

            return new KeyValuePair<string, CraftListInfo>(keyString, craftInfo);
        }

        private static KeyValuePair<string, CraftListInfo>[] GetKeyValuePairStrCraftListInfo_ArrayFromBytes(
            Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new KeyValuePair<string, CraftListInfo>[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetKeyValuePairStrCraftListInfo_FromBytes(messageData);
            return outputData;
        }
    }
}