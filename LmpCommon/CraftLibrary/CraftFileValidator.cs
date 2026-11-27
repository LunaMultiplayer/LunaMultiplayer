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

            using (var reader = new StringReader(Encoding.UTF8.GetString(data, 0, numBytes)))
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
