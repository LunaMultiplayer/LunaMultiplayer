namespace LunaClient.Systems.Flag
{
    public class FlagRespondMessage
    {
        public string FlagName { get; set; }
        public byte[] FlagData { get; set; }
        public FlagInfo FlagInfo { get; set; } = new FlagInfo();
    }

    public class FlagInfo
    {
        public string ShaSum { get; set; }
        public string Owner { get; set; }
    }
}