using LunaCommon.Groups;
using LunaCommon.Locks;
using LunaCommon.Message.Data.CraftLibrary;
using System.Collections.Generic;
using System.IO;

namespace LunaCommon.Message.Serialization
{
    public static partial class DataDeserializer
    {
        private static Group GetGroupFromBytes(Stream messageData)
        {
            var group = new Group();
            PrivDeserialize(messageData, group);

            return group;
        }

        private static Group[] GetGroupArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new Group[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetGroupFromBytes(messageData);

            return outputData;
        }

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

        private static LockDefinition GetLockDefinitionFromBytes(Stream messageData)
        {
            var lockDefinition = new LockDefinition();
            PrivDeserialize(messageData, lockDefinition);

            return lockDefinition;
        }

        private static LockDefinition[] GetLockDefinitionArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new LockDefinition[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetLockDefinitionFromBytes(messageData);
            return outputData;
        }
    }
}