using System.Collections.Generic;

namespace LunaCommon.ModFile.Structure
{
    public class MandatoryDllFile
    {
        public string FilePath { get; set; }
        public string Sha { get; set; }
    }
    
    public class ModControlStructure
    {
        public bool AllowNonListedPlugins { get; set; } = true;
        public List<MandatoryDllFile> MandatoryPlugins { get; set; } = new List<MandatoryDllFile>();
        public List<string> ForbiddenPlugins { get; set; } = new List<string>();
        public List<string> AllowedParts { get; set; } = new List<string>();
    }
}
