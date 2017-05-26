using System;
using System.Collections.Generic;
using System.IO;

namespace LunaCommon.Message.Serialization
{
    public static partial class DataDeserializer
    {
        private static short[] GetShortArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new short[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetShortFromBytes(messageData);
            return outputData;
        }

        private static int[] GetIntArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new int[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetIntFromBytes(messageData);
            return outputData;
        }

        private static uint[] GetUintArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new uint[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetUintFromBytes(messageData);
            return outputData;
        }

        private static long[] GetLongArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new long[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetLongFromBytes(messageData);
            return outputData;
        }

        private static float[] GetFloatArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new float[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetFloatFromBytes(messageData);
            return outputData;
        }

        private static double[] GetDoubleArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new double[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetDoubleFromBytes(messageData);
            return outputData;
        }

        private static bool[] GetBoolArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new bool[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetBoolFromBytes(messageData);
            return outputData;
        }

        private static string[] GetStringArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new string[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetStringFromBytes(messageData);
            return outputData;
        }

        private static Guid[] GetGuidArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new Guid[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetGuidFromBytes(messageData);
            return outputData;
        }

        private static byte[] GetByteArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new byte[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetByteFromBytes(messageData);
            return outputData;
        }

        private static KeyValuePair<int, string>[] GetKeyValuePairIntStr_ArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new KeyValuePair<int, string>[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetKeyValuePairIntStr_FromBytes(messageData);
            return outputData;
        }

        private static KeyValuePair<string, string>[] GetKeyValuePairStrStr_ArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new KeyValuePair<string, string>[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetKeyValuePairStrStr_FromBytes(messageData);
            return outputData;
        }

        private static KeyValuePair<string, string[]>[] GetKeyValuePairStrStrArray_ArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new KeyValuePair<string, string[]>[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetKeyValuePairStrStrArray_FromBytes(messageData);
            return outputData;
        }

        private static KeyValuePair<string, byte[]>[] GetKeyValuePairStrByteArray_ArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new KeyValuePair<string, byte[]>[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetKeyValuePairStrByteArray_FromBytes(messageData);
            return outputData;
        }

        private static KeyValuePair<Guid, byte[]>[] GetKeyValuePairGuidByteArray_ArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new KeyValuePair<Guid, byte[]>[numberOfElements];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetKeyValuePairGuidByteArray_FromBytes(messageData);
            return outputData;
        }

        private static byte[][] GetJaggedByteArrayFromBytes(Stream messageData)
        {
            var numberOfElements = GetIntFromBytes(messageData);
            var outputData = new byte[numberOfElements][];
            for (var element = 0; element < numberOfElements; element++)
                outputData[element] = GetByteArrayFromBytes(messageData);
            return outputData;
        }
    }
}