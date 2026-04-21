using LmpCommon.Enums;

namespace LmpClient.Systems.CraftLibrary
{
    public class CraftBasicEntry
    {
        public string FolderName { get; set; }
        public CraftType CraftType { get; set; }
        public string CraftName { get; set; }
    }

    public class CraftEntry
    {
        // Identifying information for the craft
        public string FolderName { get; set; }
        public CraftType CraftType { get; set; }
        public string CraftName { get; set; }

        // Craft data - represents the .craft file
        public int CraftNumBytes { get; set; }
        public byte[] CraftData { get; set; }

        // Craft Info data - represents the .loadmeta file
        public int CraftInfoNumBytes { get; set; }
        public byte[] CraftInfoData { get; set; }
    }
}
