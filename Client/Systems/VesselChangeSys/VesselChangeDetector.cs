using System;
using System.Linq;

namespace LunaClient.Systems.VesselChangeSys
{
    public class VesselChangeDetector
    {
        private static readonly object LockObj = new object();

        private static readonly ConfigNode Existing = new ConfigNode();
        private static readonly ConfigNode New = new ConfigNode();

        /// <summary>
        /// This method return the vessel parts that had changed and also if the stage has changed
        /// </summary>
        public static VesselChange GetProtoVesselChanges(ProtoVessel existingProtoVessel, ProtoVessel newProtoVessel)
        {
            if (existingProtoVessel == null || newProtoVessel == null)
                return null;

            //Lock this as we are using a shared ConfigNode
            lock (LockObj)
            {
                //TODO: Check if this can be improved as it probably creates a lot of garbage in memory
                var change = new VesselChange();

                Existing.ClearData();
                New.ClearData();
                existingProtoVessel.Save(Existing);
                newProtoVessel.Save(New);

                var parts1 = Existing.GetNodes("PART");
                var parts2 = New.GetNodes("PART");

                var currentStage = Existing.GetValue("stg");
                var newStage = New.GetValue("stg");

                if (currentStage != newStage)
                    change.Stage = int.Parse(newStage);

                var currentParts = parts1.Select(p => uint.Parse(p.GetValue("cid")));
                var newParts = parts2.Select(p => uint.Parse(p.GetValue("cid")));

                change.PartsToRemove = currentParts.Except(newParts).ToArray();

                var currentExtendedParts = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDeployable")
                        && m.HasValue("deployState") && m.GetValue("deployState") == "EXTENDED"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                var newExtendedParts = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDeployable")
                        && m.HasValue("deployState") && m.GetValue("deployState") == "EXTENDED"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                change.PartsToExtend = newExtendedParts.Except(currentExtendedParts).ToArray();

                var currentRetractedParts = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDeployable")
                        && m.HasValue("deployState") && m.GetValue("deployState") == "RETRACTED"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                var newRetractedParts = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDeployable")
                        && m.HasValue("deployState") && m.GetValue("deployState") == "RETRACTED"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                change.PartsToRetract = newRetractedParts.Except(currentRetractedParts).ToArray();
                
                var currentExtendedLadders = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("RetractableLadder")
                        && m.HasValue("StateName") && m.GetValue("StateName") == "Extended"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                var newExtendedLadders = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("RetractableLadder")
                        && m.HasValue("StateName") && m.GetValue("StateName") == "Extended"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                change.LaddersToExtend = newExtendedLadders.Except(currentExtendedLadders).ToArray();

                var currentRetractedLadders = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("RetractableLadder")
                        && m.HasValue("StateName") && m.GetValue("StateName") == "Retracted"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                var newRetractedLadders = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("RetractableLadder")
                        && m.HasValue("StateName") && m.GetValue("StateName") == "Retracted"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                change.LaddersToRetract = newRetractedLadders.Except(currentRetractedLadders).ToArray();

                var existingEnabledActionGrps = Existing.GetNode("ACTIONGROUPS").values.Cast<ConfigNode.Value>()
                    .Where(v => v.value.Contains("True"))
                    .Select(n => (KSPActionGroup)Enum.Parse(typeof(KSPActionGroup), n.name)).ToList();

                var newEnabledActionGrps = New.GetNode("ACTIONGROUPS").values.Cast<ConfigNode.Value>()
                    .Where(v => v.value.Contains("True"))
                    .Select(n => (KSPActionGroup)Enum.Parse(typeof(KSPActionGroup), n.name)).ToList();

                change.ActionGroupsToEnable = newEnabledActionGrps.Except(existingEnabledActionGrps).ToArray();

                var existingDisabledActionGrps = Existing.GetNode("ACTIONGROUPS").values.Cast<ConfigNode.Value>()
                    .Where(v => v.value.Contains("False"))
                    .Select(n => (KSPActionGroup)Enum.Parse(typeof(KSPActionGroup), n.name)).ToList();

                var newDisabledActionGrps = New.GetNode("ACTIONGROUPS").values.Cast<ConfigNode.Value>()
                    .Where(v => v.value.Contains("False"))
                    .Select(n => (KSPActionGroup)Enum.Parse(typeof(KSPActionGroup), n.name)).ToList();

                change.ActionGroupsToDisable = newDisabledActionGrps.Except(existingDisabledActionGrps).ToArray();

                var currentStartedEngines = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleEnginesFX")
                        && m.HasValue("EngineIgnited") && m.GetValue("EngineIgnited") == "True"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                var newStartedEngines = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleEnginesFX")
                        && m.HasValue("EngineIgnited") && m.GetValue("EngineIgnited") == "True"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                change.EnginesToStart = newStartedEngines.Except(currentStartedEngines).ToArray();

                var currentStoppedEngines = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleEnginesFX")
                        && m.HasValue("EngineIgnited") && m.GetValue("EngineIgnited") == "False"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                var newStoppedEngines = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleEnginesFX")
                        && m.HasValue("EngineIgnited") && m.GetValue("EngineIgnited") == "False"))
                    .Select(p => uint.Parse(p.GetValue("cid")));

                change.EnginesToStop = newStoppedEngines.Except(currentStoppedEngines).ToArray();

                //TODO: Fix this
                change.ShieldsToClose = new uint[0];
                change.ShieldsToOpen = new uint[0];
                //var currentOpenShields = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
                //        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDockingNode")
                //        && m.HasValue("deployState") && m.GetValue("deployState") == "RETRACTED"))
                //    .Select(p => uint.Parse(p.GetValue("cid"))).ToArray();

                //var newOpenShields = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
                //        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDockingNode")
                //        && m.HasValue("deployState") && m.GetValue("deployState") == "RETRACTED"))
                //    .Select(p => uint.Parse(p.GetValue("cid"))).ToArray();

                //var shieldsToOpen = newOpenShields.Except(currentOpenShields);

                return change;
            }
        }
    }
}
