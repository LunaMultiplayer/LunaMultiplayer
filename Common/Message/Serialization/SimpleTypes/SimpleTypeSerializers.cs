using System;
using System.Collections.Generic;
using System.IO;

namespace LunaCommon.Message.Serialization
{
    public partial class DataSerializer
    {
        private static void WriteBytesFromShort(Stream messageData, short inputData)
        {
            messageData.Write(BitConverter.GetBytes(inputData), 0, sizeof(short));
        }

        private static void WriteBytesFromUShort(Stream messageData, ushort inputData)
        {
            messageData.Write(BitConverter.GetBytes(inputData), 0, sizeof(ushort));
        }

        private static void WriteBytesFromInt(Stream messageData, int inputData)
        {
            messageData.Write(BitConverter.GetBytes(inputData), 0, sizeof(int));
        }

        private static void WriteBytesFromLong(Stream messageData, long inputData)
        {
            messageData.Write(BitConverter.GetBytes(inputData), 0, sizeof(long));
        }

        private static void WriteBytesFromFloat(Stream messageData, float inputData)
        {
            messageData.Write(BitConverter.GetBytes(inputData), 0, sizeof(float));
        }

        private static void WriteBytesFromDouble(Stream messageData, double inputData)
        {
            messageData.Write(BitConverter.GetBytes(inputData), 0, sizeof(double));
        }

        private static void WriteBytesFromBool(Stream messageData, bool inputData)
        {
            messageData.Write(BitConverter.GetBytes(inputData), 0, sizeof(bool));
        }

        private static void WriteBytesFromByte(Stream messageData, byte inputData)
        {
            var inputDataArray = new byte[sizeof(byte)];
            inputDataArray[0] = inputData;
            messageData.Write(inputDataArray, 0, sizeof(byte));
        }

        private static void WriteBytesFromString(Stream messageData, string inputData)
        {
            //Protect against empty strings
            if (inputData == null)
                inputData = "";

            var inputDataArray = BaseSerializer.Encoder.GetBytes(inputData);
            WriteBytesFromByteArray(messageData, inputDataArray);
        }

        private static void WriteBytesFromGuid(Stream messageData, Guid inputData)
        {
            WriteBytesFromString(messageData, inputData.ToString());
        }

        private static void WriteBytesFromKeyValuePairIntStr(Stream messageData, KeyValuePair<int, string> inputData)
        {
            WriteBytesFromInt(messageData, inputData.Key);
            WriteBytesFromString(messageData, inputData.Value);
        }

        private static void WriteBytesFromKeyValuePairStrStr(Stream messageData, KeyValuePair<string, string> inputData)
        {
            WriteBytesFromString(messageData, inputData.Key);
            WriteBytesFromString(messageData, inputData.Value);
        }

        private static void WriteBytesFromKeyValuePairStrStrArray(Stream messageData, KeyValuePair<string, string[]> inputData)
        {
            WriteBytesFromString(messageData, inputData.Key);
            WriteBytesFromStringArray(messageData, inputData.Value);
        }

        private static void WriteBytesFromKeyValuePairStrByteArray(Stream messageData, KeyValuePair<string, byte[]> inputData)
        {
            WriteBytesFromString(messageData, inputData.Key);
            WriteBytesFromByteArray(messageData, inputData.Value);
        }

        private static void WriteBytesFromKeyValuePairGuidByteArray(Stream messageData, KeyValuePair<Guid, byte[]> inputData)
        {
            WriteBytesFromGuid(messageData, inputData.Key);
            WriteBytesFromByteArray(messageData, inputData.Value);
        }
    }
}