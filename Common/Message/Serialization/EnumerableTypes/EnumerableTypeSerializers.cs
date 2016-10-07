using System;
using System.Collections.Generic;
using System.IO;

namespace LunaCommon.Message.Serialization
{
    public static partial class DataSerializer
    {
        private static void WriteBytesFromShortArray(Stream messageData, short[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromShort(messageData, element);
        }

        private static void WriteBytesFromIntArray(Stream messageData, int[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromInt(messageData, element);
        }

        private static void WriteBytesFromLongArray(Stream messageData, long[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromLong(messageData, element);
        }

        private static void WriteBytesFromFloatArray(Stream messageData, float[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromFloat(messageData, element);
        }

        private static void WriteBytesFromDoubleArray(Stream messageData, double[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromDouble(messageData, element);
        }

        private static void WriteBytesFromBoolArray(Stream messageData, bool[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromBool(messageData, element);
        }

        private static void WriteBytesFromStringArray(Stream messageData, string[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromString(messageData, element);
        }

        private static void WriteBytesFromGuidArray(Stream messageData, Guid[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromGuid(messageData, element);
        }

        private static void WriteBytesFromByteArray(Stream messageData, byte[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            messageData.Write(inputData, 0, inputData.Length);
        }

        private static void WriteBytesFromKeyValuePairIntStr_Array(Stream messageData,
            KeyValuePair<int, string>[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromKeyValuePairIntStr(messageData, element);
        }

        private static void WriteBytesFromKeyValuePairStrStr_Array(Stream messageData,
            KeyValuePair<string, string>[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromKeyValuePairStrStr(messageData, element);
        }

        private static void WriteBytesFromKeyValuePairStrStrArray_Array(Stream messageData,
            KeyValuePair<string, string[]>[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromKeyValuePairStrStrArray(messageData, element);
        }

        private static void WriteBytesFromKeyValuePairStrByteArray_Array(Stream messageData, KeyValuePair<string, byte[]>[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromKeyValuePairStrByteArray(messageData, element);
        }

        private static void WriteBytesFromKeyValuePairGuidByteArray_Array(Stream messageData, KeyValuePair<Guid, byte[]>[] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromKeyValuePairGuidByteArray(messageData, element);
        }

        private static void WriteBytesFromJaggedByteArray(Stream messageData, byte[][] inputData)
        {
            WriteFirstLengthByte(messageData, inputData.Length);
            foreach (var element in inputData)
                WriteBytesFromByteArray(messageData, element);
        }
    }
}