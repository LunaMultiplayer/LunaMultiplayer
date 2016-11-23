using System.Linq;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Utilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselUpdateSys
{
    public class VesselUpdateMessageSender : SubSystem<VesselUpdateSystem>, IMessageSender
    {
        private static VesselUpdateMsgData LastMsgSent { get; set; }

        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselUpdate()
        {
            var engines = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleEngines>();

            var activeEngines = engines.Where(e => e.EngineIgnited).Select(e => e.part.craftID).ToArray();
            var stoppedEngines = engines.Where(e => !e.EngineIgnited).Select(e => e.part.craftID).ToArray();
            var decouplers = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDecouple>()
                .Where(e => !e.isDecoupled).Select(e => e.part.craftID).ToArray();
            var anchoredDecouplers = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAnchoredDecoupler>()
                .Where(e => !e.isDecoupled).Select(e => e.part.craftID).ToArray();
            var clamps = FlightGlobals.ActiveVessel.FindPartModulesImplementing<LaunchClamp>().Select(e => e.part.craftID).ToArray();
            var docks = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDockingNode>().Where(e => !e.IsDisabled).Select(e => e.part.craftID).ToArray();

            var actionGrpControls = new[]
            {
                FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Gear],
                FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Light],
                FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Brakes],
                FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.SAS],
                FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.RCS]
            };

            var flightState = new FlightCtrlState();
            flightState.CopyFrom(FlightGlobals.ActiveVessel.ctrlState);

            var msg = new VesselUpdateMsgData
            {
                Stage = FlightGlobals.ActiveVessel.currentStage,
                ActiveEngines = activeEngines,
                StoppedEngines = stoppedEngines,
                Decouplers = decouplers,
                AnchoredDecouplers = anchoredDecouplers,
                Clamps = clamps,
                Docks = docks,
                VesselId = FlightGlobals.ActiveVessel.id,
                ActiongroupControls = actionGrpControls,
            };

            if (MsgIsDifferentThanLastMsgSent(msg))
            {
                SendMessage(msg);
                LastMsgSent = msg;
            }
        }

        private bool MsgIsDifferentThanLastMsgSent(VesselUpdateMsgData msg)
        {
            return LastMsgSent.Stage == msg.Stage && LastMsgSent.VesselId == msg.VesselId &&
                   CommonUtil.ScrambledEquals(LastMsgSent.ActiveEngines, msg.ActiveEngines) &&
                   CommonUtil.ScrambledEquals(LastMsgSent.StoppedEngines, msg.StoppedEngines) &&
                   CommonUtil.ScrambledEquals(LastMsgSent.Decouplers, msg.Decouplers) &&
                   CommonUtil.ScrambledEquals(LastMsgSent.AnchoredDecouplers, msg.AnchoredDecouplers) &&
                   CommonUtil.ScrambledEquals(LastMsgSent.Clamps, msg.Clamps) &&
                   CommonUtil.ScrambledEquals(LastMsgSent.Docks, msg.Docks) &&
                   CommonUtil.ScrambledEquals(LastMsgSent.ActiongroupControls, msg.ActiongroupControls);
        }
    }
}
