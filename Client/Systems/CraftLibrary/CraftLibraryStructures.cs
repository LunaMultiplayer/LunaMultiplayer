using LunaCommon.Enums;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftBasicEntry
    {
        public string FolderName { get; set; }
        public CraftType CraftType { get; set; }
        public string CraftName { get; set; }
    }

    public class CraftEntry
    {
        public string FolderName { get; set; }
        public CraftType CraftType { get; set; }
        public string CraftName { get; set; }
        public int CraftNumBytes { get; set; }
        public byte[] CraftData { get; set; }
    }
}
