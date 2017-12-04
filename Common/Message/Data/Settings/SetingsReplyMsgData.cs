using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Settings
{
    public class SettingsReplyMsgData : SettingsBaseMsgData
    {
        /// <inheritdoc />
        internal SettingsReplyMsgData() { }
        public override SettingsMessageType SettingsMessageType => SettingsMessageType.Reply;

        public WarpMode WarpMode { get; set; }
        public GameMode GameMode { get; set; }
        public TerrainQuality TerrainQuality { get; set; }
        public bool AllowCheats { get; set; }
        public int MaxNumberOfAsteroids { get; set; }
        public string ConsoleIdentifier { get; set; }
        public GameDifficulty GameDifficulty { get; set; }
        public float SafetyBubbleDistance { get; set; }
        public int VesselUpdatesSendMsInterval { get; set; }
        public int SecondaryVesselUpdatesSendMsInterval { get; set; }
        public bool AllowStockVessels { get; set; }
        public bool CanQuickLoad { get; set; }
        public bool AutoHireCrews { get; set; }
        public bool BypassEntryPurchaseAfterResearch { get; set; }
        public bool IndestructibleFacilities { get; set; }
        public bool MissingCrewsRespawn { get; set; }
        public float ReentryHeatScale { get; set; }
        public float ResourceAbundance { get; set; }
        public float FundsGainMultiplier { get; set; }
        public float FundsLossMultiplier { get; set; }
        public float RepGainMultiplier { get; set; }
        public float RepLossMultiplier { get; set; }
        public float RepLossDeclined { get; set; }
        public float ScienceGainMultiplier { get; set; }
        public float StartingFunds { get; set; }
        public float StartingReputation { get; set; }
        public float StartingScience { get; set; }
        public float RespawnTimer { get; set; }
        public bool EnableCommNet { get; set; }
        public bool EnableKerbalExperience { get; set; }
        public bool ImmediateLevelUp { get; set; }
        public bool ResourceTransferObeyCrossfeed { get; set; }
        public float BuildingImpactDamageMult { get; set; }
        public bool PartUpgradesInCareerAndSandbox { get; set; }
        public bool RequireSignalForControl { get; set; }
        public float DsnModifier { get; set; }
        public float RangeModifier { get; set; }
        public float OcclusionMultiplierVac { get; set; }
        public float OcclusionMultiplierAtm { get; set; }
        public bool EnableGroundStations { get; set; }
        public bool PlasmaBlackout { get; set; }
        public bool ActionGroupsAlways { get; set; }
        public bool GKerbalLimits { get; set; }
        public bool GPartLimits { get; set; }
        public bool PressurePartLimits { get; set; }
        public float KerbalGToleranceMult { get; set; }
        public bool AllowNegativeCurrency { get; set; }
        public bool DropControlOnVesselSwitching { get; set; }
        public bool DropControlOnExit { get; set; }
        public bool DropControlOnExitFlight { get; set; }
        public string WarpMaster { get; set; }
        public int VesselDefinitionSendMsInterval { get; set; }
        public bool ShowVesselsInThePast { get; set; }
    }
}