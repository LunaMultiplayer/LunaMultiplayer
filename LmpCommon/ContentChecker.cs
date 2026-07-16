using System;
using System.IO;
using System.Text;

namespace LmpCommon
{
    /// <summary>
    /// This class is intended to compare contents BEFORE writing them to a file. This will avoid overwriting a file with the same information
    /// This is done because the lifespan of SDcards (such as the ones used on a raspberry pi) and also SSD hardrives are reduced when writing into them
    /// </summary>
    public static class ContentChecker
    {
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

            using (var fileStream = File.OpenRead(pathToFile))
            {
                var buffer = new byte[4096];
                int totalRead = 0;
                int read;
                while ((read = fileStream.Read(buffer, 0, Math.Min(buffer.Length, numBytes - totalRead))) > 0)
                {
                    for (var i = 0; i < read; i++)
                    {
                        if (buffer[i] != contents[totalRead + i])
                            return false;
                    }
                    totalRead += read;
                }

                return totalRead == numBytes;
            }
        }

    }
}
