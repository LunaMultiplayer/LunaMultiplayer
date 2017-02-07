using System.Collections.Concurrent;
using System.Linq;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselUpdateSys
{
    public class VesselUpdateMessageHandler : SubSystem<VesselUpdateSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselUpdateMsgData;

            if (msgData == null || !System.UpdateSystemReady || VesselCommon.UpdateIsForOwnVessel(msgData.VesselId))
            {
                return;
            }

            HandleVesselUpdate(msgData);
        }

        private static void HandleVesselUpdate(VesselUpdateMsgData msg)
        {
            var vessel = FlightGlobals.FindVessel(msg.VesselId);

            if (vessel == null || !vessel.loaded) return;

            vessel.ActionGroups.SetGroup(KSPActionGroup.Gear, msg.ActiongroupControls[0]);
            vessel.ActionGroups.SetGroup(KSPActionGroup.Light, msg.ActiongroupControls[1]);
            vessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, msg.ActiongroupControls[2]);
            vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, msg.ActiongroupControls[3]);
            vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, msg.ActiongroupControls[4]);

            if (vessel.currentStage != msg.Stage)
            {
                vessel.ActionGroups.ToggleGroup(KSPActionGroup.Stage);
                vessel.currentStage = msg.Stage;
            }
            else
            {
                var engines = vessel.FindPartModulesImplementing<ModuleEngines>();
                var enginesToActivate = engines.Where(e => !e.EngineIgnited && msg.ActiveEngines.Contains(e.part.craftID));
                var enginesToStop = engines.Where(e => e.EngineIgnited && msg.StoppedEngines.Contains(e.part.craftID));

                var decouplersToLaunch = vessel.FindPartModulesImplementing<ModuleDecouple>()
                    .Where(d => !d.isDecoupled && !msg.Decouplers.Contains(d.part.craftID));

                var anchoredDecouplersToLaunch = vessel.FindPartModulesImplementing<ModuleAnchoredDecoupler>()
                    .Where(d => !d.isDecoupled && !msg.Decouplers.Contains(d.part.craftID));

                var clamps = vessel.FindPartModulesImplementing<LaunchClamp>().Where(c => !msg.Clamps.Contains(c.part.craftID));

                var docks = vessel.FindPartModulesImplementing<ModuleDockingNode>().Where(d => !d.IsDisabled && !msg.Docks.Contains(d.part.craftID));

                var shieldedDocks = vessel.FindPartModulesImplementing<ModuleDockingNode>().Where(d => !d.IsDisabled && d.deployAnimator != null);

                var shieldedDocksToToggle = shieldedDocks.Where(
                        d => (d.deployAnimator.animSwitch && msg.OpenedShieldedDocks.Contains(d.part.craftID)) ||
                             (!d.deployAnimator.animSwitch && msg.ClosedShieldedDocks.Contains(d.part.craftID)));

                foreach (var engine in enginesToActivate)
                {
                    engine?.Activate();
                }

                foreach (var engine in enginesToStop)
                {
                    engine?.Shutdown();
                }

                foreach (var decoupler in decouplersToLaunch)
                {
                    decoupler?.Decouple();
                }

                foreach (var anchoredDecoupler in anchoredDecouplersToLaunch)
                {
                    anchoredDecoupler?.Decouple();
                }

                foreach (var clamp in clamps)
                {
                    clamp?.Release();
                }

                foreach (var dock in docks)
                {
                    dock?.Decouple();
                }

                foreach (var shieldedDock in shieldedDocksToToggle)
                {
                    shieldedDock?.deployAnimator?.Toggle();
                }
            }
        }
    }
}
