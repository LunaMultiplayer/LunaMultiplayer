using LmpCommon.ModFile.Structure;
using LmpCommon.Xml;

namespace LmpCommon.ModFile
{
    public class ModFileParser
    {
        public static ModControlStructure ReadModFileFromString(string contents)
        {
            return LunaXmlSerializer.ReadXmlFromString<ModControlStructure>(contents);
        }
    }
}
