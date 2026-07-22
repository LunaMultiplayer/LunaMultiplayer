using System.Collections.Generic;

namespace LmpClient.Systems.ShareContracts
{
    /// <summary>
    /// Strips <c>CONTRACT</c> / <c>CONTRACT_FINISHED</c> children from an incoming
    /// <c>ContractSystem</c> scenario whose serialized parameters reference parts
    /// not present in this client's <see cref="PartLoader"/>.
    ///
    /// Without this, KSP's stock scenario loader hands the raw <see cref="ConfigNode"/>
    /// to ContractConfigurator's <c>PartValidation</c> parameter loader, which throws
    /// <c>ArgumentException</c> and surfaces the in-game exception popup once per
    /// offending contract.
    ///
    /// Safe to apply on the client without affecting server-side state because
    /// <c>ContractSystem</c> is in <c>IgnoredScenarios.IgnoreSend</c>, so clients never
    /// echo their (sanitized) view back to the server. Live contract changes flow
    /// through <c>ShareContractsSystem</c>'s incremental message path, not the bulk
    /// scenario sync this sanitizer runs against.
    /// </summary>
    public static class ContractsScenarioSanitizer
    {
        /// <summary>
        /// Module name of the KSP scenario this sanitizer applies to. Exposed so the
        /// scenario loader can decide whether to invoke the sanitizer without reaching
        /// into private string constants.
        /// </summary>
        public const string ScenarioModuleName = "ContractSystem";

        private const string ContractsParentNodeName = "CONTRACTS";
        private const string ActiveContractNodeName = "CONTRACT";
        private const string FinishedContractNodeName = "CONTRACT_FINISHED";

        /// <summary>
        /// Removes any contract child whose parameters reference a part this client's
        /// <see cref="PartLoader"/> does not know about. Idempotent and a no-op when
        /// the node has no <c>CONTRACTS</c> parent.
        /// </summary>
        public static void StripContractsReferencingUnknownParts(ConfigNode contractSystemScenario)
        {
            if (contractSystemScenario == null) return;

            var contractsParent = contractSystemScenario.GetNode(ContractsParentNodeName);
            if (contractsParent == null) return;

            RemoveChildrenReferencingUnknownParts(contractsParent, ActiveContractNodeName);
            RemoveChildrenReferencingUnknownParts(contractsParent, FinishedContractNodeName);
        }

        private static void RemoveChildrenReferencingUnknownParts(ConfigNode parent, string childName)
        {
            // KSP's ConfigNode.RemoveNode mutates the underlying list, so we collect
            // first to avoid concurrent-modification issues during enumeration.
            List<ConfigNode> toRemove = null;

            foreach (var child in parent.GetNodes(childName))
            {
                if (!ContractPartReferenceChecker.TryFindUnknownPartReference(child, out var unknownPart))
                    continue;

                if (toRemove == null) toRemove = new List<ConfigNode>();
                toRemove.Add(child);

                var contractType = child.GetValue("type") ?? "<unknown>";
                LunaLog.Log(
                    $"[LMP]: Stripping {childName} '{contractType}' from received ContractSystem scenario; " +
                    $"references part '{unknownPart}' which is not installed on this client.");
            }

            if (toRemove == null) return;

            foreach (var node in toRemove)
            {
                parent.RemoveNode(node);
            }
        }
    }
}
