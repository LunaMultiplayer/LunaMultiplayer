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
            SettingsSystem.ServerSettings.AllowCheats = msgData.AllowCheats;
            SettingsSystem.ServerSettings.MaxNumberOfAsteroids = msgData.MaxNumberOfAsteroids;
            SettingsSystem.ServerSettings.ConsoleIdentifier = msgData.ConsoleIdentifier;
            SettingsSystem.ServerSettings.SafetyBubbleDistance = msgData.SafetyBubbleDistance;
            SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval = msgData.VesselUpdatesSendMsInterval;
            SettingsSystem.ServerSettings.SecondaryVesselUpdatesSendMsInterval = msgData.SecondaryVesselUpdatesSendMsInterval;
            SettingsSystem.ServerSettings.DropControlOnVesselSwitching = msgData.DropControlOnVesselSwitching;
            SettingsSystem.ServerSettings.DropControlOnExitFlight = msgData.DropControlOnExitFlight;
            SettingsSystem.ServerSettings.StrandedVesselsCheckMsInterval = msgData.StrandedVesselsCheckMsInterval;
            SettingsSystem.ServerSettings.VesselDefinitionSendMsInterval = msgData.VesselDefinitionSendMsInterval;
            SettingsSystem.ServerSettings.VesselDefinitionSendFarMsInterval = msgData.VesselDefinitionSendFarMsInterval;
            SettingsSystem.ServerSettings.AbandonedVesselsUpdateMsInterval = msgData.AbandonedVesselsUpdateMsInterval;
            SettingsSystem.ServerSettings.ShowVesselsInThePast = msgData.ShowVesselsInThePast;
            SettingsSystem.ServerSettings.ClockSetMsInterval = msgData.ClockSetMsInterval;
            SettingsSystem.ServerSettings.WarpMaster = msgData.WarpMaster;
            SettingsSystem.ServerSettings.DropControlOnExit = msgData.DropControlOnExit;
            SettingsSystem.ServerSettings.GameDifficulty = msgData.GameDifficulty;

            if (SettingsSystem.ServerSettings.GameDifficulty != GameDifficulty.Custom)
            {
                SettingsSystem.ServerSettings.ServerParameters =
                    GameParameters.GetDefaultParameters(
                        SystemsContainer.Get<MainSystem>().ConvertGameMode(SettingsSystem.ServerSettings.GameMode),
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

            MainSystem.NetworkState = ClientState.SettingsSynced;
        }
    }
}