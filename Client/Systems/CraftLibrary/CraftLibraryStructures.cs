using LunaCommon.Enums;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftChangeEntry
    {
        public string PlayerName { get; set; }
        public CraftType CraftType { get; set; }
        public string CraftName { get; set; }
    }

    public class CraftResponseEntry
    {
        public string PlayerName { get; set; }
        public CraftType CraftType { get; set; }
        public string CraftName { get; set; }
        public byte[] CraftData { get; set; }
        public int CraftNumBytes { get; set; }
    }
}