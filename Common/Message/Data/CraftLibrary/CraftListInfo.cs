namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftListInfo
    {
        public bool VabExists { get; set; }
        public bool SphExists { get; set; }
        public bool SubassemblyExists { get; set; }
        public string[] VabCraftNames { get; set; }
        public string[] SphCraftNames { get; set; }
        public string[] SubassemblyCraftNames { get; set; }
    }
}