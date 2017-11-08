using LunaCommon.Enums;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using LunaServer.Settings;

namespace LunaServer.Message.Reader
{
    public class SettingsMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<SettingsReplyMsgData>();
            msgData.WarpMode = GeneralSettings.SettingsStore.WarpMode;
            msgData.GameMode = GeneralSettings.SettingsStore.GameMode;
            msgData.AllowCheats = GeneralSettings.SettingsStore.Cheats;
            msgData.MaxNumberOfAsteroids = GeneralSettings.SettingsStore.NumberOfAsteroids;
            msgData.ConsoleIdentifier = GeneralSettings.SettingsStore.ConsoleIdentifier;
            msgData.GameDifficulty = GeneralSettings.SettingsStore.GameDifficulty;
            msgData.SafetyBubbleDistance = GeneralSettings.SettingsStore.SafetyBubbleDistance;
            msgData.VesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.VesselUpdatesSendMsInterval;
            msgData.SecondaryVesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.SecondaryVesselUpdatesSendMsInterval;
            msgData.DropControlOnVesselSwitching = GeneralSettings.SettingsStore.DropControlOnVesselSwitching;
            msgData.DropControlOnExitFlight = GeneralSettings.SettingsStore.DropControlOnExitFlight;
            msgData.DropControlOnExit = GeneralSettings.SettingsStore.DropControlOnExit;
            msgData.SendScenarioDataMsInterval = GeneralSettings.SettingsStore.SendScenarioDataMsInterval;
            msgData.StrandedVesselsCheckMsInterval = GeneralSettings.SettingsStore.StrandedVesselsCheckMsInterval;
            msgData.VesselDefinitionSendMsInterval = GeneralSettings.SettingsStore.VesselDefinitionSendMsInterval;
            msgData.VesselDefinitionSendFarMsInterval = GeneralSettings.SettingsStore.VesselDefinitionSendFarMsInterval;
            msgData.AbandonedVesselsUpdateMsInterval = GeneralSettings.SettingsStore.AbandonedVesselsUpdateMsInterval;
            msgData.ShowVesselsInThePast = GeneralSettings.SettingsStore.ShowVesselsInThePast;
            msgData.ClockSetMsInterval = GeneralSettings.SettingsStore.ClockSetMsInterval;
            msgData.WarpMaster = GeneralSettings.SettingsStore.WarpMaster;

            if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.Custom)
            {
                msgData.EnableCommNet = GameplaySettings.SettingsStore.CommNetwork;
                msgData.RespawnTimer = GameplaySettings.SettingsStore.RespawnTime;
                msgData.AllowStockVessels = GameplaySettings.SettingsStore.AllowStockVessels;
                msgData.AutoHireCrews = GameplaySettings.SettingsStore.AutoHireCrews;
                msgData.BypassEntryPurchaseAfterResearch = GameplaySettings.SettingsStore.BypassEntryPurchaseAfterResearch;
                msgData.IndestructibleFacilities = GameplaySettings.SettingsStore.IndestructibleFacilities;
                msgData.MissingCrewsRespawn = GameplaySettings.SettingsStore.MissingCrewsRespawn;
                msgData.ReentryHeatScale = GameplaySettings.SettingsStore.ReentryHeatScale;
                msgData.ResourceAbundance = GameplaySettings.SettingsStore.ResourceAbundance;
                msgData.FundsGainMultiplier = GameplaySettings.SettingsStore.FundsGainMultiplier;
                msgData.CanQuickLoad = GameplaySettings.SettingsStore.CanQuickLoad;
                msgData.RepLossDeclined = GameplaySettings.SettingsStore.RepLossDeclined;
                msgData.FundsLossMultiplier = GameplaySettings.SettingsStore.FundsLossMultiplier;
                msgData.RepGainMultiplier = GameplaySettings.SettingsStore.RepGainMultiplier;
                msgData.RepLossMultiplier = GameplaySettings.SettingsStore.RepLossMultiplier;
                msgData.ScienceGainMultiplier = GameplaySettings.SettingsStore.ScienceGainMultiplier;
                msgData.StartingFunds = GameplaySettings.SettingsStore.StartingFunds;
                msgData.StartingReputation = GameplaySettings.SettingsStore.StartingReputation;
                msgData.StartingScience = GameplaySettings.SettingsStore.StartingScience;
                //Advanced
                msgData.ActionGroupsAlways = GameplaySettings.SettingsStore.ActionGroupsAlways;
                msgData.GKerbalLimits = GameplaySettings.SettingsStore.GKerbalLimits;
                msgData.GPartLimits = GameplaySettings.SettingsStore.GPartLimits;
                msgData.KerbalGToleranceMult = GameplaySettings.SettingsStore.KerbalGToleranceMult;
                msgData.PressurePartLimits = GameplaySettings.SettingsStore.PressurePartLimits;
                msgData.EnableKerbalExperience = GameplaySettings.SettingsStore.KerbalExp;
                msgData.ImmediateLevelUp = GameplaySettings.SettingsStore.ImmediateLevelUp;
                msgData.AllowNegativeCurrency = GameplaySettings.SettingsStore.AllowNegativeCurrency;
                msgData.ResourceTransferObeyCrossfeed = GameplaySettings.SettingsStore.ObeyCrossfeedRules;
                msgData.BuildingImpactDamageMult = GameplaySettings.SettingsStore.BuildingDamageMultiplier;
                msgData.PartUpgradesInCareerAndSandbox = GameplaySettings.SettingsStore.PartUpgrades;
                //Commnet
                msgData.RequireSignalForControl = GameplaySettings.SettingsStore.RequireSignalForControl;
                msgData.DsnModifier = GameplaySettings.SettingsStore.DsnModifier;
                msgData.RangeModifier = GameplaySettings.SettingsStore.RangeModifier;
                msgData.OcclusionMultiplierVac = GameplaySettings.SettingsStore.OcclusionModifierVac;
                msgData.OcclusionMultiplierAtm = GameplaySettings.SettingsStore.OcclusionModifierAtm;
                msgData.EnableGroundStations = GameplaySettings.SettingsStore.ExtraGroundstations;
                msgData.PlasmaBlackout = GameplaySettings.SettingsStore.ReentryBlackout;
            }

            MessageQueuer.SendToClient<SetingsSrvMsg>(client, msgData);
        }
    }
}