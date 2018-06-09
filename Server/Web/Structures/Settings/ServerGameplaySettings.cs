using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerGameplaySettings
    {
        public bool AllowStockVessels => GameplaySettings.SettingsStore.AllowStockVessels;
        public bool AutoHireCrews => GameplaySettings.SettingsStore.AutoHireCrews;
        public bool BypassEntryPurchaseAfterResearch => GameplaySettings.SettingsStore.BypassEntryPurchaseAfterResearch;
        public bool CanRevert => GameplaySettings.SettingsStore.CanRevert;
        public bool CommNetwork => GameplaySettings.SettingsStore.CommNetwork;
        public float RespawnTime => GameplaySettings.SettingsStore.RespawnTime;
        public float FundsGainMultiplier => GameplaySettings.SettingsStore.FundsGainMultiplier;
        public float FundsLossMultiplier => GameplaySettings.SettingsStore.FundsLossMultiplier;
        public bool IndestructibleFacilities => GameplaySettings.SettingsStore.IndestructibleFacilities;
        public bool MissingCrewsRespawn => GameplaySettings.SettingsStore.MissingCrewsRespawn;
        public float ReentryHeatScale => GameplaySettings.SettingsStore.ReentryHeatScale;
        public float RepGainMultiplier => GameplaySettings.SettingsStore.RepGainMultiplier;
        public float RepLossDeclined => GameplaySettings.SettingsStore.RepLossDeclined;
        public float RepLossMultiplier => GameplaySettings.SettingsStore.RepLossMultiplier;
        public float ResourceAbundance => GameplaySettings.SettingsStore.ResourceAbundance;
        public float ScienceGainMultiplier => GameplaySettings.SettingsStore.ScienceGainMultiplier;
        public float StartingFunds => GameplaySettings.SettingsStore.StartingFunds;
        public float StartingReputation => GameplaySettings.SettingsStore.StartingReputation;
        public float StartingScience => GameplaySettings.SettingsStore.StartingScience;
        public bool AllowNegativeCurrency => GameplaySettings.SettingsStore.AllowNegativeCurrency;
        public bool PressurePartLimits => GameplaySettings.SettingsStore.PressurePartLimits;
        public float KerbalGToleranceMult => GameplaySettings.SettingsStore.KerbalGToleranceMult;
        public bool GPartLimits => GameplaySettings.SettingsStore.GPartLimits;
        public bool GKerbalLimits => GameplaySettings.SettingsStore.GKerbalLimits;
        public bool ActionGroupsAlways => GameplaySettings.SettingsStore.ActionGroupsAlways;
        public bool KerbalExp => GameplaySettings.SettingsStore.KerbalExp;
        public bool ImmediateLevelUp => GameplaySettings.SettingsStore.ImmediateLevelUp;
        public bool ObeyCrossfeedRules => GameplaySettings.SettingsStore.ObeyCrossfeedRules;
        public float BuildingDamageMultiplier => GameplaySettings.SettingsStore.BuildingDamageMultiplier;
        public bool PartUpgrades => GameplaySettings.SettingsStore.PartUpgrades;
        public bool RequireSignalForControl => GameplaySettings.SettingsStore.RequireSignalForControl;
        public float RangeModifier => GameplaySettings.SettingsStore.RangeModifier;
        public float DsnModifier => GameplaySettings.SettingsStore.DsnModifier;
        public float OcclusionModifierVac => GameplaySettings.SettingsStore.OcclusionModifierVac;
        public float OcclusionModifierAtm => GameplaySettings.SettingsStore.OcclusionModifierAtm;
        public bool ExtraGroundstations => GameplaySettings.SettingsStore.ExtraGroundstations;
        public bool ReentryBlackout => GameplaySettings.SettingsStore.ReentryBlackout;
    }
}
