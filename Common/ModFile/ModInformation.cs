using System.Collections.Generic;

namespace LunaCommon.ModFile
{
    public class ModInformation
    {
        public List<ModItem> RequiredFiles { get; } = new List<ModItem>();
        public List<ModItem> OptionalFiles { get; } = new List<ModItem>();
        public List<ModItem> BlackListFiles { get; } = new List<ModItem>();
        public List<ModItem> WhiteListFiles { get; } = new List<ModItem>();
        public List<string> PartList { get; } = new List<string>();

        public void Clear()
        {
            RequiredFiles.Clear();
            OptionalFiles.Clear();
            BlackListFiles.Clear();
            WhiteListFiles.Clear();
            PartList.Clear();
        }
    }
}
