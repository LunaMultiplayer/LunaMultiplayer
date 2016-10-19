using System;
using System.Collections.Generic;
using System.IO;

namespace LunaCommon.Message.Serialization
{
    public partial class DataDeserializer
    {
        private static ushort GetUShortFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(ushort));

            var outputData = new byte[sizeof(ushort)];
            messageData.Read(outputData, 0, sizeof(ushort));

            return BitConverter.ToUInt16(outputData, 0);
        }

        private static short GetShortFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(short));

            var outputData = new byte[sizeof(short)];
            messageData.Read(outputData, 0, sizeof(short));

            return BitConverter.ToInt16(outputData, 0);
        }

        private static int GetIntFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(int));

            var outputData = new byte[sizeof(int)];
            messageData.Read(outputData, 0, sizeof(int));

            return BitConverter.ToInt32(outputData, 0);
        }

        private static uint GetUintFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(uint));

            var outputData = new byte[sizeof(uint)];
            messageData.Read(outputData, 0, sizeof(uint));

            return BitConverter.ToUInt32(outputData, 0);
        }

        private static long GetLongFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(long));

            var outputData = new byte[sizeof(long)];
            messageData.Read(outputData, 0, sizeof(long));

            return BitConverter.ToInt64(outputData, 0);
        }

        private static float GetFloatFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(float));

            var outputData = new byte[sizeof(float)];
            messageData.Read(outputData, 0, sizeof(float));

            return BitConverter.ToSingle(outputData, 0);
        }

        private static double GetDoubleFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(double));

            var outputData = new byte[sizeof(double)];
            messageData.Read(outputData, 0, sizeof(double));

            return BitConverter.ToDouble(outputData, 0);
        }

        private static bool GetBoolFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(bool));

            var outputData = new byte[sizeof(bool)];
            messageData.Read(outputData, 0, sizeof(bool));

            return BitConverter.ToBoolean(outputData, 0);
        }

        private static byte GetByteFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(byte));

            var outputData = new byte[sizeof(byte)];
            messageData.Read(outputData, 0, sizeof(byte));

            return outputData[0];
        }

        private static string GetStringFromBytes(Stream messageData)
        {
            var outputData = GetByteArrayFromBytes(messageData);
            var outputString = BaseSerializer.Encoder.GetString(outputData);
            return outputString;
        }

        private static Guid GetGuidFromBytes(Stream messageData)
        {
            return new Guid(GetStringFromBytes(messageData));
        }

        private static KeyValuePair<int, string> GetKeyValuePairIntStr_FromBytes(Stream messageData)
        {
            var keyInt = GetIntFromBytes(messageData);

            var outputData = GetByteArrayFromBytes(messageData);
            var valueString = BaseSerializer.Encoder.GetString(outputData);

            return new KeyValuePair<int, string>(keyInt, valueString);
        }

        private static KeyValuePair<string, string> GetKeyValuePairStrStr_FromBytes(Stream messageData)
        {
            var outputData = GetByteArrayFromBytes(messageData);
            var keyString = BaseSerializer.Encoder.GetString(outputData);

            outputData = GetByteArrayFromBytes(messageData);
            var valueString = BaseSerializer.Encoder.GetString(outputData);

            return new KeyValuePair<string, string>(keyString, valueString);
        }

        private static KeyValuePair<string, string[]> GetKeyValuePairStrStrArray_FromBytes(Stream messageData)
        {
            var outputData = GetByteArrayFromBytes(messageData);
            var keyString = BaseSerializer.Encoder.GetString(outputData);

            var valueArray = GetStringArrayFromBytes(messageData);

            return new KeyValuePair<string, string[]>(keyString, valueArray);
        }

        private static KeyValuePair<string, byte[]> GetKeyValuePairStrByteArray_FromBytes(Stream messageData)
        {
            var outputData = GetByteArrayFromBytes(messageData);
            var keyString = BaseSerializer.Encoder.GetString(outputData);

            var valueBytes = GetByteArrayFromBytes(messageData);

            return new KeyValuePair<string, byte[]>(keyString, valueBytes);
        }

        private static KeyValuePair<Guid, byte[]> GetKeyValuePairGuidByteArray_FromBytes(Stream messageData)
        {
            var outputData = GetGuidFromBytes(messageData);
            var valueBytes = GetByteArrayFromBytes(messageData);

            return new KeyValuePair<Guid, byte[]>(outputData, valueBytes);
        }
    }
}