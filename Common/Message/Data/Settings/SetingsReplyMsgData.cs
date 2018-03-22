using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Settings
{
    public class SettingsReplyMsgData : SettingsBaseMsgData
    {
        /// <inheritdoc />
        internal SettingsReplyMsgData() { }
        public override SettingsMessageType SettingsMessageType => SettingsMessageType.Reply;

        public WarpMode WarpMode;
        public GameMode GameMode;
        public bool ShareProgress;
        public TerrainQuality TerrainQuality;
        public bool AllowCheats;
        public bool AllowSackKerbals;
        public int MaxNumberOfAsteroids;
        public string ConsoleIdentifier;
        public GameDifficulty GameDifficulty;
        public float SafetyBubbleDistance;
        public int VesselUpdatesSendMsInterval;
        public int SecondaryVesselUpdatesSendMsInterval;
        public bool AllowStockVessels;
        public bool CanQuickLoad;
        public bool AutoHireCrews;
        public bool BypassEntryPurchaseAfterResearch;
        public bool IndestructibleFacilities;
        public bool MissingCrewsRespawn;
        public float ReentryHeatScale;
        public float ResourceAbundance;
        public float FundsGainMultiplier;
        public float FundsLossMultiplier;
        public float RepGainMultiplier;
        public float RepLossMultiplier;
        public float RepLossDeclined;
        public float ScienceGainMultiplier;
        public float StartingFunds;
        public float StartingReputation;
        public float StartingScience;
        public float RespawnTimer;
        public bool EnableCommNet;
        public bool EnableKerbalExperience;
        public bool ImmediateLevelUp;
        public bool ResourceTransferObeyCrossfeed;
        public float BuildingImpactDamageMult;
        public bool PartUpgradesInCareerAndSandbox;
        public bool RequireSignalForControl;
        public float DsnModifier;
        public float RangeModifier;
        public float OcclusionMultiplierVac;
        public float OcclusionMultiplierAtm;
        public bool EnableGroundStations;
        public bool PlasmaBlackout;
        public bool ActionGroupsAlways;
        public bool GKerbalLimits;
        public bool GPartLimits;
        public bool PressurePartLimits;
        public float KerbalGToleranceMult;
        public bool AllowNegativeCurrency;
        public string WarpMaster;
        public int VesselPartsSyncMsInterval;
        public bool ShowVesselsInThePast;
        public int MinScreenshotIntervalMs;
        public int MinCraftLibraryRequestIntervalMs;

        public override string ClassName { get; } = nameof(SettingsReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write((int)WarpMode);
            lidgrenMsg.Write((int)GameMode);
            lidgrenMsg.Write(ShareProgress);
            lidgrenMsg.Write((int)TerrainQuality);
            lidgrenMsg.Write(AllowCheats);
            lidgrenMsg.Write(AllowSackKerbals);
            lidgrenMsg.Write(MaxNumberOfAsteroids);
            lidgrenMsg.Write(ConsoleIdentifier);
            lidgrenMsg.Write((int)GameDifficulty);
            lidgrenMsg.Write(SafetyBubbleDistance);
            lidgrenMsg.Write(VesselUpdatesSendMsInterval);
            lidgrenMsg.Write(SecondaryVesselUpdatesSendMsInterval);
            lidgrenMsg.Write(AllowStockVessels);
            lidgrenMsg.Write(CanQuickLoad);
            lidgrenMsg.Write(AutoHireCrews);
            lidgrenMsg.Write(BypassEntryPurchaseAfterResearch);
            lidgrenMsg.Write(IndestructibleFacilities);
            lidgrenMsg.Write(MissingCrewsRespawn);
            lidgrenMsg.Write(ReentryHeatScale);
            lidgrenMsg.Write(ResourceAbundance);
            lidgrenMsg.Write(FundsGainMultiplier);
            lidgrenMsg.Write(FundsLossMultiplier);
            lidgrenMsg.Write(RepGainMultiplier);
            lidgrenMsg.Write(RepLossMultiplier);
            lidgrenMsg.Write(RepLossDeclined);
            lidgrenMsg.Write(ScienceGainMultiplier);
            lidgrenMsg.Write(StartingFunds);
            lidgrenMsg.Write(StartingReputation);
            lidgrenMsg.Write(StartingScience);
            lidgrenMsg.Write(RespawnTimer);
            lidgrenMsg.Write(EnableCommNet);
            lidgrenMsg.Write(EnableKerbalExperience);
            lidgrenMsg.Write(ImmediateLevelUp);
            lidgrenMsg.Write(ResourceTransferObeyCrossfeed);
            lidgrenMsg.Write(BuildingImpactDamageMult);
            lidgrenMsg.Write(PartUpgradesInCareerAndSandbox);
            lidgrenMsg.Write(RequireSignalForControl);
            lidgrenMsg.Write(DsnModifier);
            lidgrenMsg.Write(RangeModifier);
            lidgrenMsg.Write(OcclusionMultiplierVac);
            lidgrenMsg.Write(OcclusionMultiplierAtm);
            lidgrenMsg.Write(EnableGroundStations);
            lidgrenMsg.Write(PlasmaBlackout);
            lidgrenMsg.Write(ActionGroupsAlways);
            lidgrenMsg.Write(GKerbalLimits);
            lidgrenMsg.Write(GPartLimits);
            lidgrenMsg.Write(PressurePartLimits);
            lidgrenMsg.Write(KerbalGToleranceMult);
            lidgrenMsg.Write(AllowNegativeCurrency);
            lidgrenMsg.Write(WarpMaster);
            lidgrenMsg.Write(VesselPartsSyncMsInterval);
            lidgrenMsg.Write(ShowVesselsInThePast);
            lidgrenMsg.Write(MinScreenshotIntervalMs);
            lidgrenMsg.Write(MinCraftLibraryRequestIntervalMs);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            WarpMode = (WarpMode)lidgrenMsg.ReadInt32();
            GameMode = (GameMode)lidgrenMsg.ReadInt32();
            ShareProgress = lidgrenMsg.ReadBoolean();
            TerrainQuality = (TerrainQuality)lidgrenMsg.ReadInt32();
            AllowCheats = lidgrenMsg.ReadBoolean();
            AllowSackKerbals = lidgrenMsg.ReadBoolean();
            MaxNumberOfAsteroids = lidgrenMsg.ReadInt32();
            ConsoleIdentifier = lidgrenMsg.ReadString();
            GameDifficulty = (GameDifficulty)lidgrenMsg.ReadInt32();
            SafetyBubbleDistance = lidgrenMsg.ReadFloat();
            VesselUpdatesSendMsInterval = lidgrenMsg.ReadInt32();
            SecondaryVesselUpdatesSendMsInterval = lidgrenMsg.ReadInt32();
            AllowStockVessels = lidgrenMsg.ReadBoolean();
            CanQuickLoad = lidgrenMsg.ReadBoolean();
            AutoHireCrews = lidgrenMsg.ReadBoolean();
            BypassEntryPurchaseAfterResearch = lidgrenMsg.ReadBoolean();
            IndestructibleFacilities = lidgrenMsg.ReadBoolean();
            MissingCrewsRespawn = lidgrenMsg.ReadBoolean();
            ReentryHeatScale = lidgrenMsg.ReadFloat();
            ResourceAbundance = lidgrenMsg.ReadFloat();
            FundsGainMultiplier = lidgrenMsg.ReadFloat();
            FundsLossMultiplier = lidgrenMsg.ReadFloat();
            RepGainMultiplier = lidgrenMsg.ReadFloat();
            RepLossMultiplier = lidgrenMsg.ReadFloat();
            RepLossDeclined = lidgrenMsg.ReadFloat();
            ScienceGainMultiplier = lidgrenMsg.ReadFloat();
            StartingFunds = lidgrenMsg.ReadFloat();
            StartingReputation = lidgrenMsg.ReadFloat();
            StartingScience = lidgrenMsg.ReadFloat();
            RespawnTimer = lidgrenMsg.ReadFloat();
            EnableCommNet = lidgrenMsg.ReadBoolean();
            EnableKerbalExperience = lidgrenMsg.ReadBoolean();
            ImmediateLevelUp = lidgrenMsg.ReadBoolean();
            ResourceTransferObeyCrossfeed = lidgrenMsg.ReadBoolean();
            BuildingImpactDamageMult = lidgrenMsg.ReadFloat();
            PartUpgradesInCareerAndSandbox = lidgrenMsg.ReadBoolean();
            RequireSignalForControl = lidgrenMsg.ReadBoolean();
            DsnModifier = lidgrenMsg.ReadFloat();
            RangeModifier = lidgrenMsg.ReadFloat();
            OcclusionMultiplierVac = lidgrenMsg.ReadFloat();
            OcclusionMultiplierAtm = lidgrenMsg.ReadFloat();
            EnableGroundStations = lidgrenMsg.ReadBoolean();
            PlasmaBlackout = lidgrenMsg.ReadBoolean();
            ActionGroupsAlways = lidgrenMsg.ReadBoolean();
            GKerbalLimits = lidgrenMsg.ReadBoolean();
            GPartLimits = lidgrenMsg.ReadBoolean();
            PressurePartLimits = lidgrenMsg.ReadBoolean();
            KerbalGToleranceMult = lidgrenMsg.ReadFloat();
            AllowNegativeCurrency = lidgrenMsg.ReadBoolean();
            WarpMaster = lidgrenMsg.ReadString();
            VesselPartsSyncMsInterval = lidgrenMsg.ReadInt32();
            ShowVesselsInThePast = lidgrenMsg.ReadBoolean();
            MinScreenshotIntervalMs = lidgrenMsg.ReadInt32();
            MinCraftLibraryRequestIntervalMs = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(WarpMode) + sizeof(GameMode) + sizeof(TerrainQuality) + sizeof(GameDifficulty) + 
                sizeof(bool) * 22 + sizeof(int) * 6 + sizeof(float) * 19 + ConsoleIdentifier.GetByteCount() + WarpMaster.GetByteCount();
        }
    }
}
