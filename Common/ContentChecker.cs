using System;
using System.IO;
using System.Text;

namespace LunaCommon
{
    /// <summary>
    /// This class is intended to compare contents BEFORE writing them to a file. This will avoid overwriting a file with the same information
    /// This is done because the lifespan of SDcards (such as the ones used on a raspberry pi) and also SSD hardrives are reduced when writing into them
    /// </summary>
    public static class ContentChecker
    {
        private const int BytesToRead = sizeof(long);

        /// <summary>
        /// Checks if the file contents and the string are equal
        /// </summary>
        public static bool ContentsAreEqual(string contents, string pathToFile)
        {
            var contentsAsByteArr = Encoding.UTF8.GetBytes(contents);
            return ContentsAreEqual(contentsAsByteArr, contentsAsByteArr.Length, pathToFile);
        }

        /// <summary>
        /// Checks if the file contents and the byte array are equal
        /// </summary>
        public static bool ContentsAreEqual(byte[] contents, int numBytes, string pathToFile)
        {
            if (!File.Exists(pathToFile))
                return false;

            var fileInfo = new FileInfo(pathToFile);

            if (numBytes != fileInfo.Length)
                return false;

            var iterations = (int)Math.Ceiling((double)numBytes / BytesToRead);

            using (var contentStream = new MemoryStream(contents, 0, numBytes))
            using (var fileStream = File.OpenRead(pathToFile))
            {
                var one = new byte[BytesToRead];
                var two = new byte[BytesToRead];

                for (var i = 0; i < iterations; i++)
                {
                    contentStream.Read(one, 0, BytesToRead);
                    fileStream.Read(two, 0, BytesToRead);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }

    }
}
