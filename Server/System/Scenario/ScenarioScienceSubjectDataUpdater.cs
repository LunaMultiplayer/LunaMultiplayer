using LmpCommon.Message.Data.ShareProgress;
using Server.Log;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a science subject message so update the scenario file accordingly.
        /// For full payloads (IsDelta=false) we upsert the ConfigNode.
        /// For delta payloads (IsDelta=true) we only patch the numeric fields on the existing node.
        /// In both cases we apply merge logic: a lower science value never overwrites a higher one.
        /// </summary>
        public static void WriteScienceSubjectDataToFile(ScienceSubjectInfo scienceSubject)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                    {
                        if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var scenario)) return;

                        if (scienceSubject.IsDelta)
                            ApplyDeltaToExistingSubject(scenario, scienceSubject);
                        else
                            UpsertSubjectFromConfigNode(scenario, scienceSubject);
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error updating science subject scenario data: {e}");
                }
            });
        }

        /// <summary>
        /// Processes a revert snapshot from a client reverting to launch.
        /// We apply merge logic: science never decreases if another player already submitted a higher value.
        /// This protects other players' science from being rolled back by an unrelated revert.
        /// </summary>
        public static void WriteScienceSubjectRevertToFile(ShareProgressScienceSubjectRevertMsgData revert)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                    {
                        if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var scenario)) return;

                        for (var i = 0; i < revert.SubjectCount; i++)
                        {
                            var subject = revert.Subjects[i];
                            if (subject == null) continue;
                            // Reuse normal delta path — merge logic prevents rolling back other players' science
                            ApplyDeltaToExistingSubject(scenario, subject);
                        }
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error writing science subject revert to scenario: {e}");
                }
            });
        }

        // ------------------------------------------------------------------
        // Private helpers
        // ------------------------------------------------------------------

        private static void UpsertSubjectFromConfigNode(LunaConfigNode.CfgNode.ConfigNode scenario, ScienceSubjectInfo subject)
        {
            var receivedNode = ParseClientConfigNode(subject.Data, subject.NumBytes, "Science");
            receivedNode.Parent = scenario;
            if (receivedNode.IsEmpty()) return;

            var receivedId = receivedNode.GetValue("id");
            if (receivedId == null)
            {
                LunaLog.Error("Science subject update received with no id — skipping");
                return;
            }

            var scienceNodes = scenario.GetNodes("Science").Select(v => v.Value);
            var specificNode = scienceNodes.FirstOrDefault(n =>
            {
                var id = n.GetValue("id");
                return id != null && id.Value == receivedId.Value;
            });

            if (specificNode != null)
            {
                // Merge: never reduce science
                var currentSciStr = specificNode.GetValue("sci")?.Value;
                if (currentSciStr != null
                    && float.TryParse(currentSciStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var currentSci)
                    && subject.Science < currentSci)
                {
                    LunaLog.Debug($"Ignoring full science subject for '{receivedId.Value}': incoming {subject.Science} < current {currentSci}");
                    return;
                }
                scenario.ReplaceNode(specificNode, receivedNode);
            }
            else
            {
                scenario.AddNode(receivedNode);
            }
        }

        private static void ApplyDeltaToExistingSubject(LunaConfigNode.CfgNode.ConfigNode scenario, ScienceSubjectInfo delta)
        {
            var scienceNodes = scenario.GetNodes("Science").Select(v => v.Value);
            var existingNode = scienceNodes.FirstOrDefault(n =>
            {
                var id = n.GetValue("id");
                return id != null && id.Value == delta.Id;
            });

            if (existingNode == null)
            {
                LunaLog.Warning($"Delta update for unknown science subject '{delta.Id}' — skipped (subject not yet discovered)");
                return;
            }

            // Merge: never reduce science
            var currentSciStr = existingNode.GetValue("sci")?.Value;
            if (currentSciStr != null
                && float.TryParse(currentSciStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var currentSci)
                && delta.Science < currentSci)
            {
                LunaLog.Debug($"Ignoring delta for '{delta.Id}': incoming {delta.Science} < current {currentSci}");
                return;
            }

            PatchSubjectNode(existingNode, delta);
        }

        private static void PatchSubjectNode(LunaConfigNode.CfgNode.ConfigNode node, ScienceSubjectInfo delta)
        {
            node.UpdateValue("sci", delta.Science.ToString(CultureInfo.InvariantCulture));
            node.UpdateValue("cap", delta.ScienceCap.ToString(CultureInfo.InvariantCulture));
            node.UpdateValue("scv", delta.ScientificValue.ToString(CultureInfo.InvariantCulture));
            node.UpdateValue("sbv", delta.SubjectValue.ToString(CultureInfo.InvariantCulture));
        }
    }
}
