using System.ComponentModel;

namespace LunaServer.Settings
{
    public class GameplaySettingsStore
    {
        [Description("Allow Stock Vessels")]
        public bool AllowStockVessels = false;

        [Description("Auto-Hire Crewmemebers before Flight")]
        public bool AutoHireCrews = true;

        [Description("No Entry Purchase Required on Research")]
        public bool BypassEntryPurchaseAfterResearch = true;

        [Description("Allow Quickloading and Reverting Flights\n" +
                     "Note that if set to true and warp mode isn't SUBSPACE, it will have no effect")]
        public bool CanQuickLoad = true;

        [Description("Enable Comm Network")]
        public bool CommNetwork = true;

        [Description("Crew Respawn Time")]
        public float RespawnTime = 2f;

        // Career Settings
        [Description("Funds Rewards")]
        public float FundsGainMultiplier = 1.0f;

        [Description("Funds Penalties")]
        public float FundsLossMultiplier = 1.0f;

        [Description("Indestructible Facilities")]
        public bool IndestructibleFacilities = false;

        [Description("Missing Crews Respawn")]
        public bool MissingCrewsRespawn = true;

        [Description("Re-Entry Heating")]
        public float ReentryHeatScale = 1.0f;

        [Description("Reputation Rewards")]
        public float RepGainMultiplier = 1.0f;

        [Description("Decline Penalty")]
        public float RepLossDeclined = 1.0f;

        [Description("Reputation Penalties")]
        public float RepLossMultiplier = 1.0f;

        [Description("Resource Abundance")]
        public float ResourceAbundance = 1.0f;

        [Description("Science Rewards")]
        public float ScienceGainMultiplier = 1.0f;

        [Description("Starting Funds")]
        public float StartingFunds = 25000.0f;

        [Description("Starting Reputation")]
        public float StartingReputation = 0.0f;

        [Description("Starting Science")]
        public float StartingScience = 0.0f;

        // Advanced Options
        
        [Description("Allow Negative Funds/Science")]
        public bool AllowNegativeCurrency = false;

        [Description("Enable parts pressure limits")]
        public bool PressurePartLimits = false;

        [Description("Kerbal G tolerance multiplier")]
        public float KerbalGToleranceMult = 1.0f;

        [Description("Enable parts G limits")]
        public bool GPartLimits = false;

        [Description("Enable Kerbal G limits")]
        public bool GKerbalLimits = false;

        [Description("Always allow action groups")]
        public bool ActionGroupsAlways = false;

        [Description("Enable Kerbal Exp")]
        public bool KerbalExp = true;

        [Description("Kerbals Level Up Immediately")]
        public bool ImmediateLevelUp = false;

        [Description("Obey Crossfeed Rules")]
        public bool ObeyCrossfeedRules = false;

        [Description("Building Damage Multiplier")]
        public float BuildingDamageMultiplier = 0.05f;

        [Description("Part Upgrades")]
        public bool PartUpgrades = true;

        // CommNet Options
        [Description("Require Signal for Control")]
        public bool RequireSignalForControl = false;

        [Description("Range Modifier")]
        public float RangeModifier = 1.0f;

        [Description("DSN Modifier")]
        public float DsnModifier = 1.0f;

        [Description("Occlusion Modifier, Vac")]
        public float OcclusionModifierVac = 0.9f;

        [Description("Occlusion Modifier, Atm")]
        public float OcclusionModifierAtm = 0.75f;

        [Description("Enable Extra Groundstations")]
        public bool ExtraGroundstations = true;

        [Description("Enable Extra Reentry blackout on comm network")]
        public bool ReentryBlackout = true;
    }
}