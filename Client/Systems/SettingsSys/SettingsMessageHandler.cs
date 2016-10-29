using System.Collections.Concurrent;
using CommNet;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.SettingsSys
{
    public class SettingsMessageHandler : SubSystem<SettingsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as SettingsReplyMsgData;
            if (msgData == null) return;

            SettingsSystem.ServerSettings.WarpMode = msgData.WarpMode;
            SettingsSystem.ServerSettings.GameMode = msgData.GameMode;
            SettingsSystem.ServerSettings.AllowCheats = msgData.AllowCheats;
            SettingsSystem.ServerSettings.MaxNumberOfAsteroids = msgData.MaxNumberOfAsteroids;
            SettingsSystem.ServerSettings.ConsoleIdentifier = msgData.ConsoleIdentifier;
            SettingsSystem.ServerSettings.SafetyBubbleDistance = msgData.SafetyBubbleDistance;
            SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval = msgData.VesselUpdatesSendMsInterval;
            SettingsSystem.ServerSettings.DropControlOnVesselSwitching = msgData.DropControlOnVesselSwitching;
            SettingsSystem.ServerSettings.DropControlOnExitFlight = msgData.DropControlOnExitFlight;
            SettingsSystem.ServerSettings.SendScenarioDataSecInterval = msgData.SendScenarioDataSecInterval;
            SettingsSystem.ServerSettings.VesselKillCheckMsInterval = msgData.VesselKillCheckMsInterval;
            SettingsSystem.ServerSettings.StrandedVesselsCheckMsInterval = msgData.StrandedVesselsCheckMsInterval;
            SettingsSystem.ServerSettings.VesselDefinitionUpdateMsInterval = msgData.VesselDefinitionUpdateMsInterval;
            SettingsSystem.ServerSettings.AbandonedVesselsUpdateMsInterval = msgData.AbandonedVesselsUpdateMsInterval;
            SettingsSystem.ServerSettings.ClockSetMsInterval = msgData.ClockSetMsInterval;
            SettingsSystem.ServerSettings.WarpMaster = msgData.WarpMaster;
            SettingsSystem.ServerSettings.DropControlOnExit = msgData.DropControlOnExit;
            SettingsSystem.ServerSettings.GameDifficulty = msgData.GameDifficulty;

            if (SettingsSystem.ServerSettings.GameDifficulty != GameDifficulty.CUSTOM)
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
                        CanQuickLoad = msgData.CanQuickLoad,
                        CanRestart = msgData.CanQuickLoad,
                        CanLeaveToEditor = msgData.CanQuickLoad
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

            MainSystem.Singleton.NetworkState = ClientState.SETTINGS_SYNCED;
        }
    }
}