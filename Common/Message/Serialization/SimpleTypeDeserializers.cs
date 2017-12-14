using LunaCommon.Message.Base;
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

            var outputData = ArrayPool<byte>.ClaimWithExactLength(sizeof(ushort));

            messageData.Read(outputData, 0, sizeof(ushort));
            var value = BitConverter.ToUInt16(outputData, 0);

            ArrayPool<byte>.Release(ref outputData, true);

            return value;
        }

        private static short GetShortFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(short));

            var outputData = ArrayPool<byte>.ClaimWithExactLength(sizeof(short));

            messageData.Read(outputData, 0, sizeof(short));
            var value = BitConverter.ToInt16(outputData, 0);

            ArrayPool<byte>.Release(ref outputData, true);

            return value;
        }

        private static int GetIntFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(int));

            var outputData = ArrayPool<byte>.ClaimWithExactLength(sizeof(int));
            
            messageData.Read(outputData, 0, sizeof(int));
            var value = BitConverter.ToInt32(outputData, 0);

            ArrayPool<byte>.Release(ref outputData, true);
            return value;
        }

        private static uint GetUintFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(uint));

            var outputData = ArrayPool<byte>.ClaimWithExactLength(sizeof(uint));
            
            messageData.Read(outputData, 0, sizeof(uint));
            var value = BitConverter.ToUInt32(outputData, 0);

            ArrayPool<byte>.Release(ref outputData, true);

            return value;
        }

        private static long GetLongFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(long));

            var outputData = ArrayPool<byte>.ClaimWithExactLength(sizeof(long));
            
            messageData.Read(outputData, 0, sizeof(long));
            var value = BitConverter.ToInt64(outputData, 0);

            ArrayPool<byte>.Release(ref outputData, true);

            return value;
        }

        private static float GetFloatFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(float));

            var outputData = ArrayPool<byte>.ClaimWithExactLength(sizeof(float));

            messageData.Read(outputData, 0, sizeof(float));
            var value = BitConverter.ToSingle(outputData, 0);

            ArrayPool<byte>.Release(ref outputData, true);

            return value;
        }

        private static double GetDoubleFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(double));

            var outputData = ArrayPool<byte>.ClaimWithExactLength(sizeof(double));

            messageData.Read(outputData, 0, sizeof(double));
            var value = BitConverter.ToDouble(outputData, 0);

            ArrayPool<byte>.Release(ref outputData, true);

            return value;
        }

        private static bool GetBoolFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(bool));

            var outputData = ArrayPool<byte>.ClaimWithExactLength(sizeof(bool));

            messageData.Read(outputData, 0, sizeof(bool));
            var value = BitConverter.ToBoolean(outputData, 0);

            ArrayPool<byte>.Release(ref outputData, true);

            return value;
        }

        private static byte GetByteFromBytes(Stream messageData)
        {
            CheckDataLeft(messageData.Length, sizeof(byte));

            var outputData = ArrayPool<byte>.ClaimWithExactLength(sizeof(byte));

            messageData.Read(outputData, 0, sizeof(byte));
            var value = outputData[0];

            ArrayPool<byte>.Release(ref outputData, true);

            return value;
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