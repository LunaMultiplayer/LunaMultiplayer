using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace LunaCommon.Message.Serialization
{
    /// <summary>
    ///     This class provides deserialization. Instead of normal serializers it uses as less data as possible
    ///     to store the values of the properties in order to optimize bandwith.
    ///     To access the properties we use a Fast member as it's faster than reflection
    ///     We keep it static for better resource management
    /// </summary>
    public static partial class DataDeserializer
    {
        /// <summary>
        ///     Types does not accept a switch so the fastest way to access is a dictionary as defined here.
        /// </summary>
        private static readonly Dictionary<Type, Func<Stream, object>> SerializerDictionary = new Dictionary
            <Type, Func<Stream, object>>
        {
            [typeof(ushort)] = messageData => GetUShortFromBytes(messageData),
            [typeof(short)] = messageData => GetShortFromBytes(messageData),
            [typeof(int)] = messageData => GetIntFromBytes(messageData),
            [typeof(uint)] = messageData => GetUintFromBytes(messageData),
            [typeof(long)] = messageData => GetLongFromBytes(messageData),
            [typeof(float)] = messageData => GetFloatFromBytes(messageData),
            [typeof(double)] = messageData => GetDoubleFromBytes(messageData),
            [typeof(bool)] = messageData => GetBoolFromBytes(messageData),
            [typeof(byte)] = messageData => GetByteFromBytes(messageData),
            [typeof(string)] = messageData => GetStringFromBytes(messageData),
            [typeof(Guid)] = messageData => GetGuidFromBytes(messageData),
            [typeof(KeyValuePair<int, string>)] = messageData => GetKeyValuePairIntStr_FromBytes(messageData),
            [typeof(KeyValuePair<string, string>)] = messageData => GetKeyValuePairStrStr_FromBytes(messageData),
            [typeof(KeyValuePair<string, string[]>)] = messageData => GetKeyValuePairStrStrArray_FromBytes(messageData),
            [typeof(KeyValuePair<string, byte[]>)] = messageData => GetKeyValuePairStrByteArray_FromBytes(messageData),
            [typeof(KeyValuePair<Guid, byte[]>)] = messageData => GetKeyValuePairGuidByteArray_FromBytes(messageData),
            [typeof(KeyValuePair<string, CraftListInfo>)] = messageData => GetKeyValuePairStrCraftListInfo_FromBytes(messageData),
            [typeof(short[])] = messageData => GetShortArrayFromBytes(messageData),
            [typeof(int[])] = messageData => GetIntArrayFromBytes(messageData),
            [typeof(uint[])] = messageData => GetUintArrayFromBytes(messageData),
            [typeof(long[])] = messageData => GetLongArrayFromBytes(messageData),
            [typeof(float[])] = messageData => GetFloatArrayFromBytes(messageData),
            [typeof(double[])] = messageData => GetDoubleArrayFromBytes(messageData),
            [typeof(bool[])] = messageData => GetBoolArrayFromBytes(messageData),
            [typeof(byte[])] = messageData => GetByteArrayFromBytes(messageData),
            [typeof(string[])] = messageData => GetStringArrayFromBytes(messageData),
            [typeof(Guid[])] = messageData => GetGuidArrayFromBytes(messageData),
            [typeof(KeyValuePair<int, string>[])] = messageData => GetKeyValuePairIntStr_ArrayFromBytes(messageData),
            [typeof(KeyValuePair<string, string>[])] = messageData => GetKeyValuePairStrStr_ArrayFromBytes(messageData),
            [typeof(KeyValuePair<string, string[]>[])] = messageData => GetKeyValuePairStrStrArray_ArrayFromBytes(messageData),
            [typeof(KeyValuePair<string, byte[]>[])] = messageData => GetKeyValuePairStrByteArray_ArrayFromBytes(messageData),
            [typeof(KeyValuePair<Guid, byte[]>[])] = messageData => GetKeyValuePairGuidByteArray_ArrayFromBytes(messageData),
            [typeof(byte[][])] = messageData => GetJaggedByteArrayFromBytes(messageData),
            [typeof(KeyValuePair<string, CraftListInfo>[])] = messageData => GetKeyValuePairStrCraftListInfo_ArrayFromBytes(messageData)
        };

        /// <summary>
        ///     Deserializes a message data class from an array of bytes. CAUTION! data won't be deserialized if version mismatches
        /// </summary>
        /// <param name="dataClass">Data class to store the deserialized values</param>
        /// <param name="message">Message that contains the POCO data class</param>
        /// <param name="data">Array of bytes with the data</param>
        /// <returns>Implementation of a POCO data class with it's properties filled if there's not a version mismatch</returns>
        public static IMessageData Deserialize(IMessageBase message, IMessageData dataClass, byte[] data)
        {
            var messageData = new MemoryStream(data);

            //Always read Version first
            var version = GetValue(messageData, typeof(string)) as string;

            if (dataClass.Version != version)
            {
                message.VersionMismatch = true;
                //We don't continue reading as the version mismatches and properties could have changed
                return dataClass;
            }

            return PrivDeserialize(messageData, dataClass) as IMessageData;
        }

        /// <summary>
        ///     Private caller, used for recurrence
        /// </summary>
        /// <param name="messageData">Stream where data is stored</param>
        /// <param name="dataClass">Poco data class implementation</param>
        /// <returns>Implementation of a POCO data class with it's properties filled if there's not a version mismatch</returns>
        private static object PrivDeserialize(Stream messageData, object dataClass)
        {
            var properties = BaseSerializer.GetCachedProperties(dataClass.GetType());
            //We use the FastMember as it's faster than reflection
            var accessor = BaseSerializer.GetCachedTypeAccessor(dataClass.GetType());
            try
            {
                foreach (var prop in properties)
                {
                    var value = GetValue(messageData, prop.PropertyType);

                    if (prop.CanWrite) //Property with a setter 
                        accessor[dataClass, prop.Name] = value;
                    else
                    {
                        if (!prop.CanWrite && !Equals(value, accessor[dataClass, prop.Name]))
                            throw new Exception("Property without a setter where it's value mismatch");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Cannot deserialize this message with the bytes provided: {e}");
            }

            return dataClass;
        }

        /// <summary>
        ///     Get the value based on a type. Types don't accept switches and as the methods
        ///     are non static this can't be done in a better way :(
        /// </summary>
        /// <param name="messageData">Stream to read data from</param>
        /// <param name="type">Type of object to retrieve</param>
        /// <returns>Value</returns>
        private static object GetValue(Stream messageData, Type type)
        {
            //Switches don't work with types so we use the dictionary
            if (SerializerDictionary.ContainsKey(type) && type.BaseType != typeof(Enum))
                return SerializerDictionary[type].Invoke(messageData);
            if (type.BaseType == typeof(Enum))
            {
                var enumInt = GetIntFromBytes(messageData);
                var enumName = Enum.GetName(type, enumInt);

                if (string.IsNullOrEmpty(enumName))
                    throw new Exception("Enum value not valid");

                return Enum.Parse(type, enumName);
            }

            throw new IOException("Type not supported");
        }

        /// <summary>
        ///     Checks if there's still data on the buffer
        /// </summary>
        /// <param name="length">Length of the buffer</param>
        /// <param name="size">Size of the object to retrieve</param>
        private static void CheckDataLeft(long length, int size)
        {
            if (size > length)
                throw new IOException($"Cannot read past the end of the stream! Size: {size} Length: {length}");
        }
    }
}