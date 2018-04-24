using LunaCommon.Enums;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Message.Reader.Base;
using Server.Server;
using Server.Settings;
using Server.Settings.Structures;

namespace Server.Message.Reader
{
    public class SettingsMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {                    
            //We don't use this message anymore so we can recycle it
            message.Recycle();

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<SettingsReplyMsgData>();
            msgData.WarpMode = GeneralSettings.SettingsStore.WarpMode;
            msgData.GameMode = GeneralSettings.SettingsStore.GameMode;
            msgData.TerrainQuality = GeneralSettings.SettingsStore.TerrainQuality;
            msgData.AllowCheats = GeneralSettings.SettingsStore.Cheats;
            msgData.AllowAdmin = !string.IsNullOrEmpty(GeneralSettings.SettingsStore.AdminPassword);
            msgData.AllowSackKerbals = GeneralSettings.SettingsStore.AllowSackKerbals;
            msgData.MaxNumberOfAsteroids = GeneralSettings.SettingsStore.NumberOfAsteroids;
            msgData.ConsoleIdentifier = GeneralSettings.SettingsStore.ConsoleIdentifier;
            msgData.GameDifficulty = GeneralSettings.SettingsStore.GameDifficulty;
            msgData.SafetyBubbleDistance = GeneralSettings.SettingsStore.SafetyBubbleDistance;
            msgData.VesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.VesselUpdatesSendMsInterval;
            msgData.SecondaryVesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.SecondaryVesselUpdatesSendMsInterval;
            msgData.VesselPartsSyncMsInterval = GeneralSettings.SettingsStore.VesselPartsSyncMsInterval;
            msgData.ShowVesselsInThePast = GeneralSettings.SettingsStore.ShowVesselsInThePast;
            msgData.WarpMaster = GeneralSettings.SettingsStore.WarpMaster;
            msgData.MinScreenshotIntervalMs = ScreenshotSettings.SettingsStore.MinScreenshotIntervalMs;
            msgData.MinCraftLibraryRequestIntervalMs = CraftSettings.SettingsStore.MinCraftLibraryRequestIntervalMs;

            if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.Custom && GameplaySettings.SettingsStore != null)
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
                msgData.CanRevert = GameplaySettings.SettingsStore.CanRevert;
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
