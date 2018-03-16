using CommNet;
using LunaCommon.Enums;

namespace LunaClient.Systems.SettingsSys
{
    public class SettingsServerStructure
    {
        public WarpMode WarpMode { get; set; } = WarpMode.Subspace;
        public GameParameters ServerParameters { get; set; }
        public GameParameters.AdvancedParams ServerAdvancedParameters { get; set; } = new GameParameters.AdvancedParams();
        public CommNetParams ServerCommNetParameters { get; set; } = new CommNetParams();
        public GameMode GameMode { get; set; }
        public TerrainQuality TerrainQuality { get; set; }
        public bool AllowCheats { get; set; }
        public bool AllowSackKerbals { get; set; }
        public int MaxNumberOfAsteroids { get; set; }
        public string ConsoleIdentifier { get; set; } = "";
        public GameDifficulty GameDifficulty { get; set; }
        public float SafetyBubbleDistance { get; set; } = 100f;
        public int VesselUpdatesSendMsInterval { get; set; }
        public int SecondaryVesselUpdatesSendMsInterval { get; set; }
        public string WarpMaster { get; set; }
        public int VesselPartsSyncMsInterval { get; set; }
        public bool ShowVesselsInThePast { get; set; }
    }
}
