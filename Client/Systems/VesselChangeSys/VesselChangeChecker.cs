using LunaClient.Utilities;
using System.Collections.Generic;
using System.Linq;


namespace LunaClient.Systems.VesselChangeSys
{
    public class VesselChangeChecker
    {
        #region Fields

        private static List<ModuleEngines> Engines { get; } = new List<ModuleEngines>();
        private static List<ModuleDockingNode> Docks { get; } = new List<ModuleDockingNode>();
        private static List<ModuleDeployablePart> DeployableParts { get; } = new List<ModuleDeployablePart>();

        private static int Stage { get; set; }
        private static List<uint> ActiveEngines { get; } = new List<uint>();
        private static List<uint> StoppedEngines { get; } = new List<uint>();
        private static List<uint> OpenedShieldDocks { get; } = new List<uint>();
        private static List<uint> ClosedShieldDocks { get; } = new List<uint>();
        private static List<uint> DeployableExtendedParts { get; } = new List<uint>();
        private static List<uint> DeployableRetractedParts { get; } = new List<uint>();
        private static List<bool> ActionGrpControls { get; } = new List<bool>();

        private static int LastStage { get; set; }
        private static List<uint> LastActiveEngines { get; set; } = new List<uint>();
        private static List<uint> LastStoppedEngines { get; set; } = new List<uint>();
        private static List<uint> LastOpenedShieldDocks { get; set; } = new List<uint>();
        private static List<uint> LastClosedShieldDocks { get; set; } = new List<uint>();
        private static List<uint> LastDeployableExtendedParts { get; set; } = new List<uint>();
        private static List<uint> LastDeployableRetractedParts { get; set; } = new List<uint>();
        private static List<bool> LastActionGrpControls { get; set; } = new List<bool>();

        #endregion

        #region Public

        /// <summary>
        /// Checks if a vessel has changes compared to the last check.
        /// </summary>
        public bool CheckActiveVesselHasChanges()
        {
            CleanUp();
            Stage = FlightGlobals.ActiveVessel.currentStage;

            Engines.AddRange(FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleEngines>());
            Docks.AddRange(FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDockingNode>());
            DeployableParts.AddRange(FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDeployablePart>());

            ActiveEngines.AddRange(Engines.Where(e => e.EngineIgnited).Select(e => e.part.craftID).ToArray());
            StoppedEngines.AddRange(Engines.Where(e => !e.EngineIgnited).Select(e => e.part.craftID).ToArray());

            DeployableExtendedParts.AddRange(DeployableParts.Where(p => p.deployState == ModuleDeployablePart.DeployState.EXTENDED)
                .Select(e => e.part.craftID));
            DeployableRetractedParts.AddRange(DeployableParts.Where(p => p.deployState == ModuleDeployablePart.DeployState.RETRACTED)
                .Select(e => e.part.craftID));

            ClosedShieldDocks.AddRange(Docks.Where(d => !d.IsDisabled && d.deployAnimator != null && d.deployAnimator.animSwitch)
                .Select(e => e.part.craftID));
            OpenedShieldDocks.AddRange(Docks.Where(d => !d.IsDisabled && d.deployAnimator != null && !d.deployAnimator.animSwitch)
                .Select(e => e.part.craftID));

            ActionGrpControls.Add(FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Gear]);
            ActionGrpControls.Add(FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Light]);
            ActionGrpControls.Add(FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Brakes]);
            ActionGrpControls.Add(FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.SAS]);
            ActionGrpControls.Add(FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.RCS]);

            var hasChanges = PartsHaveChanged();

            CleanUpLast();
            LastStage = Stage;
            LastActiveEngines.AddRange(ActiveEngines);
            LastStoppedEngines.AddRange(StoppedEngines);
            LastOpenedShieldDocks.AddRange(OpenedShieldDocks);
            LastClosedShieldDocks.AddRange(ClosedShieldDocks);
            LastDeployableRetractedParts.AddRange(DeployableRetractedParts);
            LastDeployableExtendedParts.AddRange(DeployableExtendedParts);
            LastActionGrpControls.AddRange(ActionGrpControls);

            return hasChanges;
        }

        #endregion

        #region Private methods

        private static void CleanUp()
        {
            Stage = -1;
            Engines.Clear();
            Docks.Clear();
            DeployableParts.Clear();

            ActionGrpControls.Clear();
            ActiveEngines.Clear();
            StoppedEngines.Clear();
            OpenedShieldDocks.Clear();
            ClosedShieldDocks.Clear();
            DeployableExtendedParts.Clear();
            DeployableRetractedParts.Clear();
        }

        private static void CleanUpLast()
        {
            LastStage = -1;

            LastActionGrpControls.Clear();
            LastActiveEngines.Clear();
            LastStoppedEngines.Clear();
            LastOpenedShieldDocks.Clear();
            LastClosedShieldDocks.Clear();
            LastDeployableExtendedParts.Clear();
            LastDeployableRetractedParts.Clear();
        }

        private static bool PartsHaveChanged()
        {
            return LastStage != Stage ||
                   !CommonUtil.ScrambledEquals(LastActionGrpControls, ActionGrpControls) ||
                   !CommonUtil.ScrambledEquals(LastActiveEngines, ActiveEngines) ||
                   !CommonUtil.ScrambledEquals(LastStoppedEngines, StoppedEngines) ||
                   !CommonUtil.ScrambledEquals(LastOpenedShieldDocks, OpenedShieldDocks) ||
                   !CommonUtil.ScrambledEquals(LastClosedShieldDocks, ClosedShieldDocks) ||
                   !CommonUtil.ScrambledEquals(LastDeployableExtendedParts, DeployableExtendedParts) ||
                   !CommonUtil.ScrambledEquals(LastDeployableRetractedParts, DeployableRetractedParts);
        }

        #endregion
    }
}
