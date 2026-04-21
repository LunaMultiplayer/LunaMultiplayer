using Contracts;
using LmpCommon.Enums;
using System;
using System.Collections.Generic;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Pre-validates CONTRACT nodes inside the ContractPreLoader scenario before
    /// ContractConfigurator's <c>ContractPreLoader.OnLoad</c> iterates them.
    ///
    /// The problem: LMP injects the server's Offered contract nodes into the
    /// ContractPreLoader scenario so KSPCF can preserve them across GenerateContracts.
    /// Some of those contracts reference parameter types from mods the client does not
    /// have installed. <c>Contract.Load</c> calls <c>ContractSystem.GetParameterType</c>
    /// for each PARAM, which returns null for unknown types. The calling code in
    /// <c>Contract.Load</c> then dereferences that null, throwing a
    /// <c>NullReferenceException</c>.
    ///
    /// ContractConfigurator's ContractPreLoader catches the exception but then
    /// <em>deliberately</em> re-logs it as [EXC] via <c>LoggingUtil.LogException</c>.
    /// That means a finalizer on <c>Contract.Load</c> cannot prevent the [EXC] —
    /// ContractPreLoader's catch block has already run by the time a Harmony finalizer
    /// would see the exception inside the method frame.
    ///
    /// The only reliable fix is to remove the bad CONTRACT nodes from the
    /// ContractPreLoader scenario <em>before</em> <c>ContractPreLoader.OnLoad</c> is
    /// called.  <c>ContractSystem.Instance</c> is available at this point because
    /// ContractSystem (stock KSP) is always instantiated before ContractConfigurator's
    /// ContractPreLoader in the scenario module loading order.
    ///
    /// This class is NOT applied via <c>[HarmonyPatch]</c> attributes because
    /// <c>ContractPreLoader.OnLoad</c> is a virtual override: attribute-driven patches
    /// target the <c>ScenarioModule.OnLoad</c> base-class body, which is never
    /// dispatched through when the instance is a ContractPreLoader.  Instead,
    /// <see cref="HarmonyPatcher.PatchContractPreLoader"/> looks up the type at runtime
    /// and applies <see cref="Prefix"/> imperatively against the concrete override.
    ///
    /// Contracts removed here still go through the normal ContractSystem loading path,
    /// where <see cref="ContractSystem_LoadContract"/> suppresses the resulting
    /// exception, and <see cref="LmpClient.Systems.ShareContracts.ShareContractsEvents"/>
    /// later creates an <see cref="LmpClient.Systems.ShareContracts.LmpUnavailableContract"/>
    /// stub so the player can see which server contracts they are missing.
    /// </summary>
    public static class ContractPreLoader_Filter
    {
        /// <summary>
        /// Body-target keys: the CC parameter fields that name a celestial body. A CC PARAM
        /// must have at least one of these if it also has a body-context key.
        /// Kept in sync with <c>ScenarioSystem.BodyIndexKeys</c>.
        /// </summary>
        private static readonly HashSet<string> BodyTargetKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "body", "targetBody", "destination", "origin", "body1", "body2",
        };

        /// <summary>
        /// Keys found exclusively in CC contract parameters that also require a body-target
        /// key. If a CC PARAM has any of these but no <see cref="BodyTargetKeys"/> entry,
        /// the parameter is malformed and CC will throw <see cref="ArgumentException"/> and
        /// show an in-game popup when it tries to parse the missing required body field.
        /// Kept in sync with <c>ScenarioSystem.BodyContextKeys</c>.
        /// </summary>
        private static readonly HashSet<string> BodyContextKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "coverage", "scanType", "scanMode",
            "experiment", "biome", "situation",
            "orbitType", "minOrbit", "maxOrbit",
            "minAltitude", "maxAltitude",
            "minPeriapsis", "maxPeriapsis",
            "minApoapsis", "maxApoapsis",
            "minEccentricity", "maxEccentricity",
            "minInclination", "maxInclination",
        };

        /// <summary>
        /// Harmony prefix applied imperatively to <c>ContractPreLoader.OnLoad(ConfigNode)</c>
        /// by <see cref="HarmonyPatcher.PatchContractPreLoader"/>.
        /// </summary>
        internal static void Prefix(object __instance, ConfigNode node)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            // Guard: ensure ContractSystem has been initialised (it loads before ContractPreLoader).
            if (ContractSystem.Instance == null)
            {
                LunaLog.LogWarning("[ContractPreLoader_Filter]: ContractSystem.Instance is null — skipping pre-validation.");
                return;
            }

            FilterContracts(node);
        }

        /// <summary>
        /// Removes CONTRACT child nodes from <paramref name="scenarioNode"/> that would
        /// cause <c>Contract.Load</c> to throw (unknown parameter type or malformed CC
        /// parameter data). All other child nodes are preserved unchanged.
        /// </summary>
        private static void FilterContracts(ConfigNode scenarioNode)
        {
            // Snapshot child nodes before we modify the parent.
            var snapshot = new List<ConfigNode>(scenarioNode.nodes.Count);
            foreach (ConfigNode child in scenarioNode.nodes)
                snapshot.Add(child);

            var toKeep = new List<ConfigNode>(snapshot.Count);
            var removedCount = 0;

            foreach (var child in snapshot)
            {
                if (string.Equals(child.name, "CONTRACT", StringComparison.OrdinalIgnoreCase))
                {
                    var isCC = string.Equals(child.GetValue("type"), "ConfiguredContract", StringComparison.OrdinalIgnoreCase);
                    var reason = FindFirstInvalidParam(child, isCC);
                    if (reason != null)
                    {
                        var guid = child.GetValue("guid") ?? "unknown";
                        var type = child.GetValue("type") ?? "unknown";
                        LunaLog.LogWarning(
                            $"[LMP]: ContractPreLoader — skipping contract {guid} (type: {type}): {reason}. " +
                            $"An unavailability stub will be shown in Mission Control.");
                        removedCount++;
                        continue;
                    }
                }

                toKeep.Add(child);
            }

            if (removedCount == 0) return;

            scenarioNode.ClearNodes();
            foreach (var keep in toKeep)
                scenarioNode.AddNode(keep);

            LunaLog.Log($"[ContractPreLoader_Filter]: Removed {removedCount} contract(s) with invalid parameters from ContractPreLoader scenario.");
        }

        /// <summary>
        /// Recursively searches PARAM sub-nodes for the first parameter that would cause
        /// <c>Contract.Load</c> to throw. Returns a human-readable reason string, or
        /// <c>null</c> if all parameters look valid.
        ///
        /// Detects two failure modes:
        /// <list type="bullet">
        ///   <item>Unknown type — <c>ContractSystem.GetParameterType</c> returns null,
        ///         causing a <c>NullReferenceException</c>.</item>
        ///   <item>Missing required body field — a CC PARAM has body-context keys
        ///         (e.g. <c>coverage</c>, <c>scanType</c>) but no body-target key
        ///         (e.g. <c>targetBody</c>), which causes CC's
        ///         <c>ConfigNodeUtil.ParseValue&lt;CelestialBody&gt;</c> to throw
        ///         <c>ArgumentException</c> and show an in-game popup.</item>
        /// </list>
        /// </summary>
        private static string FindFirstInvalidParam(ConfigNode node, bool isCC)
        {
            foreach (ConfigNode child in node.nodes)
            {
                if (!string.Equals(child.name, "PARAM", StringComparison.OrdinalIgnoreCase))
                    continue;

                var typeName = child.GetValue("name");

                // Check 1: unknown parameter type (ContractSystem.GetParameterType is static).
                if (!string.IsNullOrEmpty(typeName) && ContractSystem.GetParameterType(typeName) == null)
                    return $"parameter type '{typeName}' is not installed on this client";

                // Check 2: CC PARAM with body-context field but no body-target field.
                if (isCC && !string.IsNullOrEmpty(typeName))
                {
                    var hasBodyContext = false;
                    var hasBodyTarget = false;
                    foreach (ConfigNode.Value v in child.values)
                    {
                        if (BodyContextKeys.Contains(v.name)) hasBodyContext = true;
                        if (BodyTargetKeys.Contains(v.name)) hasBodyTarget = true;
                    }

                    if (hasBodyContext && !hasBodyTarget)
                        return $"CC parameter '{typeName}' has body-context field (e.g. coverage/scanType) but is missing required body target (targetBody)";
                }

                // Recurse into nested PARAM nodes.
                var nested = FindFirstInvalidParam(child, isCC);
                if (nested != null) return nested;
            }

            return null;
        }
    }
}
