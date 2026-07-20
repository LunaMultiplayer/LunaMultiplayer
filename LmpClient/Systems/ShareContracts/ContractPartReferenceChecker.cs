namespace LmpClient.Systems.ShareContracts
{
    /// <summary>
    /// Recursively scans a contract-related <see cref="ConfigNode"/> subtree for any
    /// <c>part = &lt;name&gt;</c> value referring to a part not present in this client's
    /// <see cref="PartLoader"/>. Used to filter out shared contracts whose parameters
    /// (stock <c>PartTest</c>, ContractConfigurator's <c>PartValidation</c>, etc.) reference
    /// parts that only exist on a more heavily-modded peer.
    ///
    /// The check is intentionally limited to the literal field name <c>part</c>, which is
    /// the canonical key used by both stock contract parameters and ContractConfigurator's
    /// <c>PartValidation</c>. This keeps the helper free of any compile-time or runtime
    /// dependency on ContractConfigurator while still catching the case that produces the
    /// in-game exception popup.
    /// </summary>
    public static class ContractPartReferenceChecker
    {
        /// <summary>
        /// Returns <c>true</c> if any <c>part</c> value found anywhere in the given
        /// subtree fails to resolve via <see cref="PartLoader.getPartInfoByName"/>.
        /// The first unresolved name is returned via <paramref name="unknownPartName"/>.
        /// </summary>
        public static bool TryFindUnknownPartReference(ConfigNode node, out string unknownPartName)
        {
            unknownPartName = null;
            if (node == null) return false;

            foreach (var partName in node.GetValues("part"))
            {
                if (string.IsNullOrEmpty(partName)) continue;
                if (PartLoader.getPartInfoByName(partName) == null)
                {
                    unknownPartName = partName;
                    return true;
                }
            }

            foreach (var child in node.GetNodes())
            {
                if (TryFindUnknownPartReference(child, out unknownPartName))
                    return true;
            }

            return false;
        }
    }
}
