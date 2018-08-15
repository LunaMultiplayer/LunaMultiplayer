using LunaCommon;
using LunaCommon.Message.Data.Flag;

namespace LunaClient.Systems.Flag
{
    public class ExtendedFlagInfo : FlagInfo
    {
        public string ShaSum => Common.CalculateSha256Hash(FlagData);
        public bool Loaded { get; set; }

        public ExtendedFlagInfo(FlagInfo flagInfo)
        {
            FlagData = Common.TrimArray(flagInfo.FlagData, flagInfo.NumBytes);
            Owner = flagInfo.Owner;
            FlagName = flagInfo.FlagName;
        }
    }
}