using System;
using System.Linq;

namespace LunaClient.Systems.VesselChangeSys
{
    public class VesselChangeDetector
    {
        private static readonly object LockObj = new object();

        private static readonly ConfigNode Node1 = new ConfigNode();
        private static readonly ConfigNode Node2 = new ConfigNode();

        /// <summary>
        /// This method return the vessel parts that had changed and also if the stage has changed
        /// </summary>
        public static VesselChange GetProtoVesselChanges(ProtoVessel existing, ProtoVessel newProtoVessel)
        {
            //Lock this as we are using a shared ConfigNode
            lock (LockObj)
            {
                //TODO: Check if this can be improved as it probably creates a lot of garbage in memory
                var change = new VesselChange();

                Node1.ClearData();
                Node2.ClearData();
                existing.Save(Node1);
                newProtoVessel.Save(Node2);

                var parts1 = Node1.GetNodes("PART");
                var parts2 = Node2.GetNodes("PART");

                var currentStage = Node1.GetValue("stg");
                var newStage = Node2.GetValue("stg");

                if (currentStage != newStage)
                    change.Stage = Int32.Parse(newStage);

                var currentParts = parts1.Select(p => UInt32.Parse(p.GetValue("cid")));
                var newParts = parts2.Select(p => UInt32.Parse(p.GetValue("cid")));

                change.PartsToRemove = currentParts.Except(newParts).ToArray();

                var currentExtendedParts = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDeployable")
                        && m.HasValue("deployState") && m.GetValue("deployState") == "EXTENDED"))
                    .Select(p => UInt32.Parse(p.GetValue("cid")));

                var newExtendedParts = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDeployable")
                        && m.HasValue("deployState") && m.GetValue("deployState") == "EXTENDED"))
                    .Select(p => UInt32.Parse(p.GetValue("cid")));

                change.PartsToExtend = newExtendedParts.Except(currentExtendedParts).ToArray();

                var currentRetractedParts = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDeployable")
                        && m.HasValue("deployState") && m.GetValue("deployState") == "RETRACTED"))
                    .Select(p => UInt32.Parse(p.GetValue("cid")));

                var newRetractedParts = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
                        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDeployable")
                        && m.HasValue("deployState") && m.GetValue("deployState") == "RETRACTED"))
                    .Select(p => UInt32.Parse(p.GetValue("cid")));

                change.PartsToRetract = newRetractedParts.Except(currentRetractedParts).ToArray();

                //TODO: Fix this and extend it to detect engines on/off, open/closed shielded docks, etc
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
