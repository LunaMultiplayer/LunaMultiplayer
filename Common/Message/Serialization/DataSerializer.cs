using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace LunaCommon.Message.Serialization
{
    /// <summary>
    ///     This class provides serialization. Instead of normal serializers it uses as less data as possible
    ///     to store the values of the properties in order to optimize bandwith.
    ///     To access the properties we use a Fast member as it's faster than reflection
    ///     We keep it static for better resource management
    /// </summary>
    public static partial class DataSerializer
    {
        /// <summary>
        ///     Types does not accept a switch so the fastest way to access is a dictionary as defined here.
        /// </summary>
        private static readonly Dictionary<Type, Action<Stream, object>> DeserializerDictionary = new Dictionary
            <Type, Action<Stream, object>>
        {
            [typeof(ushort)] = (messageData, inputData) => WriteBytesFromUShort(messageData, (ushort)inputData),
            [typeof(short)] = (messageData, inputData) => WriteBytesFromShort(messageData, (short)inputData),
            [typeof(int)] = (messageData, inputData) => WriteBytesFromInt(messageData, (int)inputData),
            [typeof(uint)] = (messageData, inputData) => WriteBytesFromUint(messageData, (uint)inputData),
            [typeof(long)] = (messageData, inputData) => WriteBytesFromLong(messageData, (long)inputData),
            [typeof(float)] = (messageData, inputData) => WriteBytesFromFloat(messageData, (float)inputData),
            [typeof(double)] = (messageData, inputData) => WriteBytesFromDouble(messageData, (double)inputData),
            [typeof(bool)] = (messageData, inputData) => WriteBytesFromBool(messageData, (bool)inputData),
            [typeof(byte)] = (messageData, inputData) => WriteBytesFromByte(messageData, (byte)inputData),
            [typeof(string)] = (messageData, inputData) => WriteBytesFromString(messageData, (string)inputData),
            [typeof(Guid)] = (messageData, inputData) => WriteBytesFromGuid(messageData, (Guid)inputData),
            [typeof(KeyValuePair<int, string>)] = (messageData, inputData) =>
                    WriteBytesFromKeyValuePairIntStr(messageData, (KeyValuePair<int, string>)inputData),
            [typeof(KeyValuePair<string, string>)] = (messageData, inputData) =>
                    WriteBytesFromKeyValuePairStrStr(messageData, (KeyValuePair<string, string>)inputData),
            [typeof(KeyValuePair<string, string[]>)] = (messageData, inputData) =>
                    WriteBytesFromKeyValuePairStrStrArray(messageData, (KeyValuePair<string, string[]>)inputData),
            [typeof(KeyValuePair<string, byte[]>)] = (messageData, inputData) =>
                    WriteBytesFromKeyValuePairStrByteArray(messageData, (KeyValuePair<string, byte[]>)inputData),
            [typeof(KeyValuePair<Guid, byte[]>)] = (messageData, inputData) =>
                WriteBytesFromKeyValuePairGuidByteArray(messageData, (KeyValuePair<Guid, byte[]>)inputData),
            [typeof(KeyValuePair<string, CraftListInfo>)] = (messageData, inputData) =>
                WriteBytesFromKeyValuePairStrCraftListInfo(messageData,
                    (KeyValuePair<string, CraftListInfo>)inputData),
            [typeof(short[])] =
                (messageData, inputData) => WriteBytesFromShortArray(messageData, (short[])inputData),
            [typeof(int[])] = (messageData, inputData) => WriteBytesFromIntArray(messageData, (int[])inputData),
            [typeof(uint[])] = (messageData, inputData) => WriteBytesFromUintArray(messageData, (uint[])inputData),
            [typeof(long[])] = (messageData, inputData) => WriteBytesFromLongArray(messageData, (long[])inputData),
            [typeof(float[])] =
                (messageData, inputData) => WriteBytesFromFloatArray(messageData, (float[])inputData),
            [typeof(double[])] =
                (messageData, inputData) => WriteBytesFromDoubleArray(messageData, (double[])inputData),
            [typeof(bool[])] = (messageData, inputData) => WriteBytesFromBoolArray(messageData, (bool[])inputData),
            [typeof(byte[])] = (messageData, inputData) => WriteBytesFromByteArray(messageData, (byte[])inputData),
            [typeof(string[])] =
                (messageData, inputData) => WriteBytesFromStringArray(messageData, (string[])inputData),
            [typeof(Guid[])] = (messageData, inputData) => WriteBytesFromGuidArray(messageData, (Guid[])inputData),
            [typeof(KeyValuePair<int, string>[])] = (messageData, inputData) =>
                    WriteBytesFromKeyValuePairIntStr_Array(messageData, (KeyValuePair<int, string>[])inputData),
            [typeof(KeyValuePair<string, string>[])] = (messageData, inputData) =>
                    WriteBytesFromKeyValuePairStrStr_Array(messageData, (KeyValuePair<string, string>[])inputData),
            [typeof(KeyValuePair<string, string[]>[])] = (messageData, inputData) =>
                WriteBytesFromKeyValuePairStrStrArray_Array(messageData, (KeyValuePair<string, string[]>[])inputData),
            [typeof(KeyValuePair<string, byte[]>[])] = (messageData, inputData) =>
                WriteBytesFromKeyValuePairStrByteArray_Array(messageData, (KeyValuePair<string, byte[]>[])inputData),
            [typeof(KeyValuePair<Guid, byte[]>[])] = (messageData, inputData) =>
                WriteBytesFromKeyValuePairGuidByteArray_Array(messageData, (KeyValuePair<Guid, byte[]>[])inputData),
            [typeof(byte[][])] = (messageData, inputData) => WriteBytesFromJaggedByteArray(messageData, (byte[][])inputData),
            [typeof(KeyValuePair<string, CraftListInfo>[])] = (messageData, inputData) =>
                WriteBytesFromKeyValuePairStrCraftListInfo_Array(messageData,
                    (KeyValuePair<string, CraftListInfo>[])inputData),
            [typeof(Enum)] = (messageData, inputData) => WriteBytesFromInt(messageData, (int)inputData)
        };


        /// <summary>
        ///     Serializes a POCO data class using a high efficient system.
        /// </summary>
        /// <typeparam name="T">Message data type</typeparam>
        /// <param name="data">Message data implementation</param>
        /// <returns>Serialized array of bytes</returns>
        public static byte[] Serialize<T>(T data) where T : IMessageData
        {
            return PrivSerialize(data, new MemoryStream());
        }

        /// <summary>
        ///     Private accessor for recurrence
        /// </summary>
        /// <param name="data">Message data implementation</param>
        /// <param name="messageData">Stream to write the bytes to</param>
        /// <param name="recursive">True if you call this from recursively and want to keep the stream open</param>
        /// <returns>Serialized array of bytes</returns>
        private static byte[] PrivSerialize(object data, Stream messageData, bool recursive = false)
        {
            if (!recursive)
                messageData = new MemoryStream();

            var properties = BaseSerializer.GetCachedProperties(data.GetType());

            //We use the FastMember as it's faster than reflection
            var accessor = BaseSerializer.GetCachedTypeAccessor(data.GetType());

            //First write the version!
            if (!recursive)
                WriteValue(messageData, typeof(string), accessor[data, "Version"]);

            //Then write the other properties...
            foreach (var prop in properties)
            {
                WriteValue(messageData, prop.PropertyType, accessor[data, prop.Name]);
            }

            var array = ((MemoryStream)messageData).ToArray();

            if (!recursive)
                messageData.Dispose();

            return array;
        }

        /// <summary>
        ///     Writes the data to the stream based on a type.
        /// </summary>
        /// <param name="messageData">Memory stream where data is contained</param>
        /// <param name="type">Type of object to write</param>
        /// <param name="inputData">Value to write</param>
        private static void WriteValue(Stream messageData, Type type, object inputData)
        {
            if (inputData == null)
                inputData = DefaultNullValues(type);

            //Switches can't type type parameters so we use the dictionary
            if (DeserializerDictionary.ContainsKey(type) && type.BaseType != typeof(Enum))
                DeserializerDictionary[type].Invoke(messageData, inputData);
            else if (type.BaseType == typeof(Enum))
                DeserializerDictionary[typeof(Enum)].Invoke(messageData, inputData);
            else
                throw new IOException("Type not supported");
        }

        /// <summary>
        ///     We can't write null values so we default with this method
        /// </summary>
        /// <param name="type">Type of the value to default</param>
        /// <returns>Default value</returns>
        private static object DefaultNullValues(Type type)
        {
            object inputData;
            if (type.IsValueType)
                inputData = Activator.CreateInstance(type);
            else if (type.IsArray)
                inputData = Activator.CreateInstance(type, 0);
            else if (type == typeof(string))
                inputData = string.Empty;
            else
                throw new Exception("This tipe cannot have a null value!");
            return inputData;
        }

        /// <summary>
        ///     Writes the first length byte for an array
        /// </summary>
        /// <param name="messageData">Stream where data is contained</param>
        /// <param name="length">Length of an array</param>
        private static void WriteFirstLengthByte(Stream messageData, int length)
        {
            WriteBytesFromInt(messageData, length);
        }
    }
}