using CachedQuickLz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LmpCommon
{
    public class Common
    {
        public static void ThreadSafeCompress(object lockObj, ref byte[] data, ref int numBytes)
        {
            lock (lockObj)
            {
                if (!CachedQlz.IsCompressed(data, numBytes))
                {
                    CachedQlz.Compress(ref data, ref numBytes);
                }
            }
        }

        public static void ThreadSafeDecompress(object lockObj, ref byte[] data, int length, out int numBytes)
        {
            lock (lockObj)
            {
                if (CachedQlz.IsCompressed(data, length))
                {
                    CachedQlz.Decompress(ref data, out numBytes);
                }
                else
                {
                    numBytes = length;
                }
            }
        }

        public static T[] TrimArray<T>(T[] array, int size)
        {
            var newArray = new T[size];
            Array.Copy(array, newArray, size);
            return newArray;
        }

        public static bool PlatformIsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        /// <summary>
        /// Compare two ienumerables and return if they are the same or not IGNORING the order
        /// </summary>
        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var list1Enu = list1 as T[] ?? list1.ToArray();
            var list2Enu = list2 as T[] ?? list2.ToArray();
            if (list1Enu.Length != list2Enu.Length)
            {
                return false;
            }

            var cnt = new Dictionary<T, int>();
            foreach (var s in list1Enu)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (var s in list2Enu)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public string CalculateSha256StringHash(string input)
        {
            return CalculateSha256Hash(Encoding.UTF8.GetBytes(input));
        }

        public static string CalculateSha256FileHash(string fileName)
        {
            return CalculateSha256Hash(File.ReadAllBytes(fileName));
        }

        public static string CalculateSha256Hash(byte[] data)
        {
            using (var provider = SHA256.Create())
            {
                var hashedBytes = provider.ComputeHash(data);
                return BitConverter.ToString(hashedBytes);
            }
        }

        public static string ConvertConfigStringToGuidString(string configNodeString)
        {
            if (configNodeString == null || configNodeString.Length != 32)
                return null;
            var returnString = new string[5];
            returnString[0] = configNodeString.Substring(0, 8);
            returnString[1] = configNodeString.Substring(8, 4);
            returnString[2] = configNodeString.Substring(12, 4);
            returnString[3] = configNodeString.Substring(16, 4);
            returnString[4] = configNodeString.Substring(20);
            return string.Join("-", returnString);
        }

        public static Guid ConvertConfigStringToGuid(string configNodeString)
        {
            try
            {
                return new Guid(ConvertConfigStringToGuidString(configNodeString));
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }
    }
}