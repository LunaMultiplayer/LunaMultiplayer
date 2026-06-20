using LunaConfigNode.CfgNode;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// Sub-node that KSP's <c>ProgressNode</c> uses to persist the kerbals that participated
        /// in a given progress milestone (e.g. the <c>Reach</c> / <c>Orbit</c> / <c>FlyBy</c>
        /// children of a <c>Progress</c> entry). KSP serializes it as a list of repeated
        /// <c>item = &lt;name&gt;</c> values.
        /// </summary>
        private const string CrewSubNodeName = "crew";

        /// <summary>
        /// Key KSP uses for each entry inside a <see cref="CrewSubNodeName"/> sub-node.
        /// </summary>
        private const string CrewItemValueKey = "item";

        /// <summary>
        /// Removes duplicate <c>item = &lt;name&gt;</c> entries from every <c>crew</c> sub-node
        /// reachable from <paramref name="root"/>. Order of first occurrence is preserved, and
        /// any non-<c>item</c> values inside a <c>crew</c> node are kept verbatim.
        ///
        /// Background: KSP appends to the underlying <c>List&lt;string&gt; crew</c> on a progress
        /// node every time the milestone is touched. Across multiple play sessions, server
        /// restarts and reverts this list grows unboundedly with duplicates, which makes
        /// <c>ProgressTracking.txt</c> balloon and serialization on the client (which happens on
        /// every scene transition) take tens of seconds. See LunaMultiplayer/LunaMultiplayer#542.
        ///
        /// Implementation note: when a <c>crew</c> node has duplicates we replace it wholesale
        /// via <c>ReplaceNode</c> rather than calling <c>RemoveValue</c> per duplicate. The
        /// per-value remove is functional in LunaConfigNode 1.8.1, but mirroring the rebuild
        /// pattern already used by the contracts updater keeps us insulated from any of the
        /// known LunaConfigNode 1.8.1 collection-mutation quirks (see
        /// <see cref="MigrateContractsScenario"/>).
        /// </summary>
        public static void DedupeCrewLists(ConfigNode root)
        {
            if (root == null) return;

            // Snapshot the children before mutating: ReplaceNode on a child below would otherwise
            // invalidate the live enumeration.
            foreach (var child in root.GetAllNodes().ToList())
            {
                if (string.Equals(child.Name, CrewSubNodeName, StringComparison.Ordinal))
                {
                    DedupeSingleCrewNode(child, root);
                    continue;
                }

                DedupeCrewLists(child);
            }
        }

        /// <summary>
        /// Migrates the <c>ProgressTracking</c> scenario after it has been loaded from disk so
        /// that historical bloat written by older builds (or accumulated before the per-write
        /// dedupe in <see cref="WriteAchievementDataToFile"/> was added) is repaired in place.
        /// Safe to run on every server start: a clean scenario is a no-op.
        /// </summary>
        public static void MigrateProgressTrackingScenario(ConfigNode scenario)
        {
            if (scenario == null) return;

            var progressParent = scenario.GetNode("Progress")?.Value;
            if (progressParent == null) return;

            DedupeCrewLists(progressParent);
        }

        private static void DedupeSingleCrewNode(ConfigNode crewNode, ConfigNode parent)
        {
            var allValues = crewNode.GetAllValues()?.ToList();
            if (allValues == null || allValues.Count <= 1) return;

            var seenItems = new HashSet<string>(StringComparer.Ordinal);
            var kept = new List<CfgNodeValue<string, string>>(allValues.Count);
            var hadDuplicate = false;

            foreach (var value in allValues)
            {
                if (string.Equals(value.Key, CrewItemValueKey, StringComparison.Ordinal))
                {
                    if (!seenItems.Add(value.Value))
                    {
                        hadDuplicate = true;
                        continue;
                    }
                }

                kept.Add(value);
            }

            if (!hadDuplicate) return;

            var replacement = new ConfigNode(crewNode.Name, parent);
            foreach (var value in kept)
            {
                replacement.CreateValue(new CfgNodeValue<string, string>(value.Key, value.Value));
            }

            parent.ReplaceNode(crewNode, replacement);
        }
    }
}
