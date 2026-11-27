using System.IO;
using System.Text;

namespace LmpCommon.CraftLibrary
{
    /// <summary>
    /// Checks that a craft payload contains at least one PART block.
    /// KSP's .loadmeta summaries have none, and opening one as a craft crashes the editor.
    /// Shared by client and server so both reject the same bad payloads.
    /// </summary>
    public static class CraftFileValidator
    {
        /// <summary>
        /// Returns true if any line in the first numBytes of data is a PART block header.
        /// </summary>
        public static bool HasParts(byte[] data, int numBytes)
        {
            if (data == null || numBytes <= 0 || numBytes > data.Length) return false;

            using (var stream = new MemoryStream(data, 0, numBytes, false))
                return HasParts(stream);
        }

        /// <summary>
        /// Returns true if any line in the stream is a PART block header, reading only as far as that line.
        /// The caller keeps ownership of the stream and must close it.
        /// </summary>
        public static bool HasParts(Stream stream)
        {
            if (stream == null) return false;

            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim() == "PART") return true;
                }
            }

            return false;
        }
    }
}
