using CommNet;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.SettingsSys
{
    public class SettingsMessageHandler : SubSystem<SettingsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is SettingsReplyMsgData msgData)) return;

            SettingsSystem.ServerSettings.WarpMode = msgData.WarpMode;
            SettingsSystem.ServerSettings.GameMode = msgData.GameMode;
            SettingsSystem.ServerSettings.TerrainQuality = msgData.TerrainQuality;
            SettingsSystem.ServerSettings.AllowCheats = msgData.AllowCheats;
            SettingsSystem.ServerSettings.AllowAdmin = msgData.AllowAdmin;
            SettingsSystem.ServerSettings.AllowSackKerbals = msgData.AllowSackKerbals;
            SettingsSystem.ServerSettings.MaxNumberOfAsteroids = msgData.MaxNumberOfAsteroids;
            SettingsSystem.ServerSettings.ConsoleIdentifier = msgData.ConsoleIdentifier;
            SettingsSystem.ServerSettings.SafetyBubbleDistance = msgData.SafetyBubbleDistance;
            SettingsSystem.ServerSettings.VesselUpdatesMsInterval = msgData.VesselUpdatesMsInterval;
            SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval = msgData.SecondaryVesselUpdatesMsInterval;
            SettingsSystem.ServerSettings.ForceInterpolationOffset = msgData.ForceInterpolationOffset;
            SettingsSystem.ServerSettings.ForceInterpolation = msgData.ForceInterpolation;
            SettingsSystem.ServerSettings.ForceExtrapolation = msgData.ForceExtrapolation;
            SettingsSystem.ServerSettings.VesselPartsSyncMsInterval = msgData.VesselPartsSyncMsInterval;
            SettingsSystem.ServerSettings.ShowVesselsInThePast = msgData.ShowVesselsInThePast;
            SettingsSystem.ServerSettings.WarpMaster = msgData.WarpMaster;
            SettingsSystem.ServerSettings.GameDifficulty = msgData.GameDifficulty;
            SettingsSystem.ServerSettings.MinScreenshotIntervalMs = msgData.MinScreenshotIntervalMs;
            SettingsSystem.ServerSettings.MaxScreenshotWidth = msgData.MaxScreenshotWidth;
            SettingsSystem.ServerSettings.MaxScreenshotHeight = msgData.MaxScreenshotHeight;
            SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs = msgData.MinScreenshotIntervalMs;

            if (SettingsSystem.ServerSettings.GameDifficulty != GameDifficulty.Custom)
            {
                SettingsSystem.ServerSettings.ServerParameters =
                    GameParameters.GetDefaultParameters(
                        MainSystem.Singleton.ConvertGameMode(SettingsSystem.ServerSettings.GameMode),
                        (GameParameters.Preset)SettingsSystem.ServerSettings.GameDifficulty);
            }
            else
            {
                SettingsSystem.ServerSettings.ServerParameters = new GameParameters
                {
                    Difficulty =
                    {
                        AllowStockVessels = msgData.AllowStockVessels,
                        AutoHireCrews = msgData.AutoHireCrews,
                        BypassEntryPurchaseAfterResearch = msgData.BypassEntryPurchaseAfterResearch,
                        IndestructibleFacilities = msgData.IndestructibleFacilities,
                        MissingCrewsRespawn = msgData.MissingCrewsRespawn,
                        ReentryHeatScale = msgData.ReentryHeatScale,
                        ResourceAbundance = msgData.ResourceAbundance,
                        RespawnTimer = msgData.RespawnTimer,
                        EnableCommNet = msgData.EnableCommNet
                    },
                    Career =
                    {
                        FundsGainMultiplier = msgData.FundsGainMultiplier,
                        FundsLossMultiplier = msgData.FundsLossMultiplier,
                        RepGainMultiplier = msgData.RepGainMultiplier,
                        RepLossMultiplier = msgData.RepLossMultiplier,
                        RepLossDeclined = msgData.RepLossDeclined,
                        ScienceGainMultiplier = msgData.ScienceGainMultiplier,
                        StartingFunds = msgData.StartingFunds,
                        StartingReputation = msgData.StartingReputation,
                        StartingScience = msgData.StartingScience
                    },
                    Flight =
                    {
                        CanQuickLoad = false, //Do not allow quickload, it's useless ina  multiplayer game
                        CanRestart = msgData.CanRevert,
                        CanLeaveToEditor = msgData.CanRevert
                    }
                };

                SettingsSystem.ServerSettings.ServerAdvancedParameters = new GameParameters.AdvancedParams
                {
                    ActionGroupsAlways = msgData.ActionGroupsAlways,
                    GKerbalLimits = msgData.GKerbalLimits,
                    GPartLimits = msgData.GPartLimits,
                    KerbalGToleranceMult = msgData.KerbalGToleranceMult,
                    PressurePartLimits = msgData.PressurePartLimits,
                    AllowNegativeCurrency = msgData.AllowNegativeCurrency,
                    EnableKerbalExperience = msgData.EnableKerbalExperience,
                    ImmediateLevelUp = msgData.ImmediateLevelUp,
                    ResourceTransferObeyCrossfeed = msgData.ResourceTransferObeyCrossfeed,
                    BuildingImpactDamageMult = msgData.BuildingImpactDamageMult,
                    PartUpgradesInCareer = msgData.PartUpgradesInCareerAndSandbox,
                    PartUpgradesInSandbox = msgData.PartUpgradesInCareerAndSandbox
                };

                SettingsSystem.ServerSettings.ServerCommNetParameters = new CommNetParams
                {
                    requireSignalForControl = msgData.RequireSignalForControl,
                    DSNModifier = msgData.DsnModifier,
                    rangeModifier = msgData.RangeModifier,
                    occlusionMultiplierVac = msgData.OcclusionMultiplierVac,
                    occlusionMultiplierAtm = msgData.OcclusionMultiplierAtm,
                    enableGroundStations = msgData.EnableGroundStations,
                    plasmaBlackout = msgData.PlasmaBlackout
                };
            }

            System.AdjustLocalSettings(msgData);
            MainSystem.NetworkState = ClientState.SettingsSynced;
        }
    }
}
