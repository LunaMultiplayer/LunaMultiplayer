using CommNet;
using LunaCommon.Enums;

namespace LunaClient.Systems.SettingsSys
{
    public class SettingsServerStructure
    {
        public WarpMode WarpMode { get; set; } = WarpMode.SUBSPACE;
        public GameParameters ServerParameters { get; set; }
        public GameParameters.AdvancedParams ServerAdvancedParameters { get; set; } = new GameParameters.AdvancedParams();
        public CommNetParams ServerCommNetParameters { get; set; } = new CommNetParams();
        public GameMode GameMode { get; set; }
        public bool AllowCheats { get; set; }
        public int MaxNumberOfAsteroids { get; set; }
        public string ConsoleIdentifier { get; set; } = "";
        public GameDifficulty GameDifficulty { get; set; }
        public float SafetyBubbleDistance { get; set; } = 100f;
        public int VesselUpdatesSendMsInterval { get; set; } = 30;
        public bool DropControlOnVesselSwitching { get; set; }
        public bool DropControlOnExit { get; set; }
        public bool DropControlOnExitFlight { get; set; }
        public int SendScenarioDataSecInterval { get; set; }
        public string WarpMaster { get; set; }
        public int VesselKillCheckMsInterval { get; set; }
        public int ClockSetMsInterval { get; set; }
        public int StrandedVesselsCheckMsInterval { get; set; }
    }
}
