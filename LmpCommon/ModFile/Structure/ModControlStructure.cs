using System.Collections.Generic;

namespace LmpCommon.ModFile.Structure
{
    public class MandatoryPart
    {
        public string Text { get; set; }
        public string Link { get; set; }
        public string PartName { get; set; }
    }

    public class ForbiddenPart
    {
        public string Text { get; set; }
        public string PartName { get; set; }
    }

    public class MandatoryDllFile
    {
        public string Text { get; set; }
        public string Link { get; set; }
        public string FilePath { get; set; }
        public string Sha { get; set; }
    }

    public class ForbiddenDllFile
    {
        public string Text { get; set; }
        public string FilePath { get; set; }
    }

    public class ModControlStructure
    {
        public bool AllowNonListedPlugins { get; set; } = true;
        public List<string> RequiredExpansions { get; set; } = new List<string>();
        public List<MandatoryDllFile> MandatoryPlugins { get; set; } = new List<MandatoryDllFile>();
        public List<ForbiddenDllFile> ForbiddenPlugins { get; set; } = new List<ForbiddenDllFile>();
        public List<MandatoryPart> MandatoryParts { get; set; } = new List<MandatoryPart>();
        public List<ForbiddenPart> ForbiddenParts { get; set; } = new List<ForbiddenPart>();
        public List<string> AllowedParts { get; set; } = new List<string>();
    }
}
