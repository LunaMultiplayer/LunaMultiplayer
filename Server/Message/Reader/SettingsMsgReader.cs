using LunaCommon.Enums;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using LunaServer.Settings;

namespace LunaServer.Message.Reader
{
    public class SettingsMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var settingsData = new SettingsReplyMsgData
            {
                WarpMode = GeneralSettings.SettingsStore.WarpMode,
                GameMode = GeneralSettings.SettingsStore.GameMode,
                AllowCheats = GeneralSettings.SettingsStore.Cheats,
                MaxNumberOfAsteroids = GeneralSettings.SettingsStore.NumberOfAsteroids,
                ConsoleIdentifier = GeneralSettings.SettingsStore.ConsoleIdentifier,
                GameDifficulty = GeneralSettings.SettingsStore.GameDifficulty,
                SafetyBubbleDistance = GeneralSettings.SettingsStore.SafetyBubbleDistance,
                VesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.VesselUpdatesSendMsInterval,
                SecondaryVesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.SecondaryVesselUpdatesSendMsInterval,
                DropControlOnVesselSwitching = GeneralSettings.SettingsStore.DropControlOnVesselSwitching,
                DropControlOnExitFlight = GeneralSettings.SettingsStore.DropControlOnExitFlight,
                DropControlOnExit = GeneralSettings.SettingsStore.DropControlOnExit,
                SendScenarioDataMsInterval = GeneralSettings.SettingsStore.SendScenarioDataMsInterval,
                VesselKillCheckMsInterval = GeneralSettings.SettingsStore.VesselKillCheckMsInterval,
                StrandedVesselsCheckMsInterval = GeneralSettings.SettingsStore.StrandedVesselsCheckMsInterval,
                VesselDefinitionSendMsInterval = GeneralSettings.SettingsStore.VesselDefinitionSendMsInterval,
                VesselDefinitionSendFarMsInterval = GeneralSettings.SettingsStore.VesselDefinitionSendFarMsInterval,
                AbandonedVesselsUpdateMsInterval = GeneralSettings.SettingsStore.AbandonedVesselsUpdateMsInterval,
                ShowVesselsInThePast = GeneralSettings.SettingsStore.ShowVesselsInThePast,
                ClockSetMsInterval = GeneralSettings.SettingsStore.ClockSetMsInterval,
                WarpMaster = GeneralSettings.SettingsStore.WarpMaster,
            };

            if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.CUSTOM)
            {
                settingsData.EnableCommNet = GameplaySettings.SettingsStore.CommNetwork;
                settingsData.RespawnTimer = GameplaySettings.SettingsStore.RespawnTime;
                settingsData.AllowStockVessels = GameplaySettings.SettingsStore.AllowStockVessels;
                settingsData.AutoHireCrews = GameplaySettings.SettingsStore.AutoHireCrews;
                settingsData.BypassEntryPurchaseAfterResearch = GameplaySettings.SettingsStore.BypassEntryPurchaseAfterResearch;
                settingsData.IndestructibleFacilities = GameplaySettings.SettingsStore.IndestructibleFacilities;
                settingsData.MissingCrewsRespawn = GameplaySettings.SettingsStore.MissingCrewsRespawn;
                settingsData.ReentryHeatScale = GameplaySettings.SettingsStore.ReentryHeatScale;
                settingsData.ResourceAbundance = GameplaySettings.SettingsStore.ResourceAbundance;
                settingsData.FundsGainMultiplier = GameplaySettings.SettingsStore.FundsGainMultiplier;
                settingsData.CanQuickLoad = GameplaySettings.SettingsStore.CanQuickLoad;
                settingsData.RepLossDeclined = GameplaySettings.SettingsStore.RepLossDeclined;
                settingsData.FundsLossMultiplier = GameplaySettings.SettingsStore.FundsLossMultiplier;
                settingsData.RepGainMultiplier = GameplaySettings.SettingsStore.RepGainMultiplier;
                settingsData.RepLossMultiplier = GameplaySettings.SettingsStore.RepLossMultiplier;
                settingsData.ScienceGainMultiplier = GameplaySettings.SettingsStore.ScienceGainMultiplier;
                settingsData.StartingFunds = GameplaySettings.SettingsStore.StartingFunds;
                settingsData.StartingReputation = GameplaySettings.SettingsStore.StartingReputation;
                settingsData.StartingScience = GameplaySettings.SettingsStore.StartingScience;
                //Advanced
                settingsData.ActionGroupsAlways = GameplaySettings.SettingsStore.ActionGroupsAlways;
                settingsData.GKerbalLimits = GameplaySettings.SettingsStore.GKerbalLimits;
                settingsData.GPartLimits = GameplaySettings.SettingsStore.GPartLimits;
                settingsData.KerbalGToleranceMult = GameplaySettings.SettingsStore.KerbalGToleranceMult;
                settingsData.PressurePartLimits = GameplaySettings.SettingsStore.PressurePartLimits;
                settingsData.EnableKerbalExperience = GameplaySettings.SettingsStore.KerbalExp;
                settingsData.ImmediateLevelUp = GameplaySettings.SettingsStore.ImmediateLevelUp;
                settingsData.AllowNegativeCurrency = GameplaySettings.SettingsStore.AllowNegativeCurrency;
                settingsData.ResourceTransferObeyCrossfeed = GameplaySettings.SettingsStore.ObeyCrossfeedRules;
                settingsData.BuildingImpactDamageMult = GameplaySettings.SettingsStore.BuildingDamageMultiplier;
                settingsData.PartUpgradesInCareerAndSandbox = GameplaySettings.SettingsStore.PartUpgrades;
                //Commnet
                settingsData.RequireSignalForControl = GameplaySettings.SettingsStore.RequireSignalForControl;
                settingsData.DsnModifier = GameplaySettings.SettingsStore.DsnModifier;
                settingsData.RangeModifier = GameplaySettings.SettingsStore.RangeModifier;
                settingsData.OcclusionMultiplierVac = GameplaySettings.SettingsStore.OcclusionModifierVac;
                settingsData.OcclusionMultiplierAtm = GameplaySettings.SettingsStore.OcclusionModifierAtm;
                settingsData.EnableGroundStations = GameplaySettings.SettingsStore.ExtraGroundstations;
                settingsData.PlasmaBlackout = GameplaySettings.SettingsStore.ReentryBlackout;
            }

            MessageQueuer.SendToClient<SetingsSrvMsg>(client, settingsData);
        }
    }
}