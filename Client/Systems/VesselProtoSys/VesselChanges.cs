using System.Linq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselChanges
    {
        /// <summary>
        /// This method return the vessel parts that had changed and also if the stage has changed
        /// </summary>
        public static VesselChange GetProtoVesselChanges(ProtoVessel existing, ProtoVessel newProtoVessel)
        {
            var change = new VesselChange();

            var protoVesselNode1 = new ConfigNode();
            var protoVesselNode2 = new ConfigNode();
            existing.Save(protoVesselNode1);
            newProtoVessel.Save(protoVesselNode2);

            var parts1 = protoVesselNode1.GetNodes("PART");
            var parts2 = protoVesselNode2.GetNodes("PART");

            var currentStage = protoVesselNode1.GetValue("stg");
            var newStage = protoVesselNode2.GetValue("stg");

            if (currentStage != newStage)
                change.Stage = int.Parse(newStage);

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

            //var currentOpenShields = parts1.Where(p => p.GetNodes("MODULE").Any(m =>
            //        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDockingNode")
            //        && m.HasValue("deployState") && m.GetValue("deployState") == "RETRACTED"))
            //    .Select(p => uint.Parse(p.GetValue("cid"))).ToArray();

            //var newOpenShields = parts2.Where(p => p.GetNodes("MODULE").Any(m =>
            //        m.HasValue("name") && m.GetValue("name").StartsWith("ModuleDockingNode")
            //        && m.HasValue("deployState") && m.GetValue("deployState") == "RETRACTED"))
            //    .Select(p => uint.Parse(p.GetValue("cid"))).ToArray();

            //var shieldsToOpen = newOpenShields.Except(currentOpenShields);

            // var shieldedDocks = vessel.FindPartModulesImplementing<ModuleDockingNode>().Where(d => !d.IsDisabled && d.deployAnimator != null);
            //var closedShieldDocks = shieldedDocks.Where(d => d.deployAnimator.animSwitch).ToArray();
            //var openedShieldDocks = shieldedDocks.Where(d => !d.deployAnimator.animSwitch).ToArray();
            //Toggle....
            //shieldedDock.deployAnimator?.Toggle();

            return change;
        }
        
        public static void ProcessVesselChanges(Vessel vessel, VesselChange vesselChange)
        {
            if (vessel != null)
            {
                if (vesselChange.Stage > int.MinValue)
                {
                    vessel.ActionGroups.ToggleGroup(KSPActionGroup.Stage);
                }

                foreach (var partToExtend in vesselChange.PartsToExtend)
                {
                    var part = vessel.parts.FirstOrDefault(p => p.craftID == partToExtend);
                    if (part != null)
                    {
                        part.FindModuleImplementing<ModuleDeployablePart>().Extend();
                    }
                }

                foreach (var partToExtend in vesselChange.PartsToRetract)
                {
                    var part = vessel.parts.FirstOrDefault(p => p.craftID == partToExtend);
                    if (part != null)
                    {
                        part.FindModuleImplementing<ModuleDeployablePart>().Retract();
                    }
                }
            }
        }
    }
}
