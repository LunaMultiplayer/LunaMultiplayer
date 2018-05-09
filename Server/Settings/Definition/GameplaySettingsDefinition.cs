using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class GameplaySettingsDefinition
    {
        [XmlComment(Value = "Allow Stock Vessels")]
        public bool AllowStockVessels { get; set; } = false;

        [XmlComment(Value = "Auto-Hire Crewmemebers before Flight")]
        public bool AutoHireCrews { get; set; } = true;

        [XmlComment(Value = "No Entry Purchase Required on Research")]
        public bool BypassEntryPurchaseAfterResearch { get; set; } = true;

        [XmlComment(Value = "Allow Reverting")]
        public bool CanRevert { get; set; } = true;

        [XmlComment(Value = "Enable Comm Network")]
        public bool CommNetwork { get; set; } = true;

        [XmlComment(Value = "Crew Respawn Time")]
        public float RespawnTime { get; set; } = 2f;

        // Career Settings
        [XmlComment(Value = "Funds Rewards")]
        public float FundsGainMultiplier { get; set; } = 1.0f;

        [XmlComment(Value = "Funds Penalties")]
        public float FundsLossMultiplier { get; set; } = 1.0f;

        [XmlComment(Value = "Indestructible Facilities")]
        public bool IndestructibleFacilities { get; set; } = false;

        [XmlComment(Value = "Missing Crews Respawn")]
        public bool MissingCrewsRespawn { get; set; } = true;

        [XmlComment(Value = "Re-Entry Heating")]
        public float ReentryHeatScale { get; set; } = 1.0f;

        [XmlComment(Value = "Reputation Rewards")]
        public float RepGainMultiplier { get; set; } = 1.0f;

        [XmlComment(Value = "Decline Penalty")]
        public float RepLossDeclined { get; set; } = 1.0f;

        [XmlComment(Value = "Reputation Penalties")]
        public float RepLossMultiplier { get; set; } = 1.0f;

        [XmlComment(Value = "Resource Abundance")]
        public float ResourceAbundance { get; set; } = 1.0f;

        [XmlComment(Value = "Science Rewards")]
        public float ScienceGainMultiplier { get; set; } = 1.0f;

        [XmlComment(Value = "Starting Funds")]
        public float StartingFunds { get; set; } = 25000.0f;

        [XmlComment(Value = "Starting Reputation")]
        public float StartingReputation { get; set; } = 0.0f;

        [XmlComment(Value = "Starting Science")]
        public float StartingScience { get; set; } = 0.0f;

        // Advanced Options

        [XmlComment(Value = "Allow Negative Funds/Science")]
        public bool AllowNegativeCurrency { get; set; } = false;

        [XmlComment(Value = "Enable parts pressure limits")]
        public bool PressurePartLimits { get; set; } = false;

        [XmlComment(Value = "Kerbal G tolerance multiplier")]
        public float KerbalGToleranceMult { get; set; } = 1.0f;

        [XmlComment(Value = "Enable parts G limits")]
        public bool GPartLimits { get; set; } = false;

        [XmlComment(Value = "Enable Kerbal G limits")]
        public bool GKerbalLimits { get; set; } = false;

        [XmlComment(Value = "Always allow action groups")]
        public bool ActionGroupsAlways { get; set; } = false;

        [XmlComment(Value = "Enable Kerbal Exp")]
        public bool KerbalExp { get; set; } = true;

        [XmlComment(Value = "Kerbals Level Up Immediately")]
        public bool ImmediateLevelUp { get; set; } = false;

        [XmlComment(Value = "Obey Crossfeed Rules")]
        public bool ObeyCrossfeedRules { get; set; } = false;

        [XmlComment(Value = "Building Damage Multiplier")]
        public float BuildingDamageMultiplier { get; set; } = 0.05f;

        [XmlComment(Value = "Part Upgrades")]
        public bool PartUpgrades { get; set; } = true;

        // CommNet Options
        [XmlComment(Value = "Require Signal for Control")]
        public bool RequireSignalForControl { get; set; } = false;

        [XmlComment(Value = "Range Modifier")]
        public float RangeModifier { get; set; } = 1.0f;

        [XmlComment(Value = "DSN Modifier")]
        public float DsnModifier { get; set; } = 1.0f;

        [XmlComment(Value = "Occlusion Modifier, Vac")]
        public float OcclusionModifierVac { get; set; } = 0.9f;

        [XmlComment(Value = "Occlusion Modifier, Atm")]
        public float OcclusionModifierAtm { get; set; } = 0.75f;

        [XmlComment(Value = "Enable Extra Groundstations")]
        public bool ExtraGroundstations { get; set; } = true;

        [XmlComment(Value = "Enable Extra Reentry blackout on comm network")]
        public bool ReentryBlackout { get; set; } = true;
    }
}
