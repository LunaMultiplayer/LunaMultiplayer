using Contracts;

namespace LmpClient.Systems.ShareContracts
{
    /// <summary>
    /// A placeholder contract shown in the Available tab when the server has a contract
    /// whose type or required assets (parts, experiments, celestial bodies) are not available
    /// on this client.  The contract cannot be accepted and exists solely to inform the player
    /// of server-side content they are missing.
    ///
    /// Stubs are created transiently at scene load from <see cref="ShareContractsEvents.ContractsLoaded"/>
    /// and are never sent back to the server (ContractSystem is in IgnoredScenarios.IgnoreSend).
    /// </summary>
    public class LmpUnavailableContract : Contract
    {
        internal const string OriginalTypeKey = "lmpOriginalType";
        internal const string MissingAssetKey = "lmpMissingAsset";

        public string OriginalTypeName { get; private set; } = "Unknown";

        /// <summary>
        /// Human-readable description of the specific missing resource, e.g.:
        /// <list type="bullet">
        ///   <item>"part 'SomeModPart'" — a required part is not in PartLoader.</item>
        ///   <item>"body index #6" — a celestial body index is out of range (planet pack mismatch).</item>
        /// </list>
        /// Null when the contract type itself is unknown (missing mod) rather than a specific asset.
        /// </summary>
        public string MissingAsset { get; private set; }

        // Never auto-generate new instances of this type.
        protected override bool Generate() => false;

        public override bool MeetRequirements() => false;

        protected override void OnLoad(ConfigNode node)
        {
            OriginalTypeName = node.GetValue(OriginalTypeKey) ?? "Unknown";
            MissingAsset = node.GetValue(MissingAssetKey);
        }

        protected override void OnSave(ConfigNode node)
        {
            node.AddValue(OriginalTypeKey, OriginalTypeName);
            if (MissingAsset != null)
                node.AddValue(MissingAssetKey, MissingAsset);
        }

        protected override string GetTitle()
            => $"[Not Available] {OriginalTypeName}";

        protected override string GetDescription()
            => MissingAsset != null
                ? $"This contract requires {MissingAsset}, which is not available on this client. " +
                  $"The server has mod content (parts, planet packs, etc.) that you do not have installed."
                : $"This contract requires the content type \"{OriginalTypeName}\" which is not installed on " +
                  $"this client. It was offered on the server using a mod you do not have.";

        protected override string GetSynopsys()
            => "Requires mod content not installed on this client.";

        protected override string MessageCompleted() => string.Empty;
    }
}
