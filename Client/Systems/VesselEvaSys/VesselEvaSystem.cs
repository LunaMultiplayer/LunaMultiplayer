using LunaClient.Base;
using LunaClient.VesselUtilities;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LunaClient.Systems.VesselEvaSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselEvaSystem : MessageSystem<VesselEvaSystem, VesselEvaMessageSender, VesselEvaMessageHandler>
    {
        #region Fields & properties

        public bool EvaSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT && Time.timeSinceLevelLoad > 1f;

        public FieldInfo[] FsmStates { get; } = typeof(KerbalEVA)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(KFSMEvent)).ToArray();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselEvaSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, SetEvaFsmCallback));
        }

        #endregion

        /// <summary>
        /// Here we check if our active vessel is a EVA and if so we set a callback 
        /// </summary>
        private void SetEvaFsmCallback()
        {
            if (EvaSystemReady)
            {
                if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || !FlightGlobals.ActiveVessel.isEVA)
                    return;

                var kerbalEva = FlightGlobals.ActiveVessel.FindPartModuleImplementing<KerbalEVA>();
                if (kerbalEva != null)
                {
                    if (!kerbalEva.fsm.OnStateChange.GetInvocationList().Any(h => h.Method.Name == nameof(LmpOnStateChange)))
                        kerbalEva.fsm.OnStateChange += LmpOnStateChange;
                }
            }
        }

        /// <summary>
        /// Whenever we receive a eva message we run this method. 
        /// </summary>
        public void RunEvent(Vessel evaVessel, string newFsmState, string eventToRun)
        {
            if (evaVessel == null || FsmStates == null || FsmStates.Length == 0) return;

            //First update the fsm state in the protovessel as perhaps it's unloaded
            UpdateFsmStateInProtoVessel(evaVessel.protoVessel, newFsmState);

            var kerbalEva = evaVessel.FindPartModuleImplementing<KerbalEVA>();
            if (kerbalEva == null || kerbalEva.fsm == null) return;

            //Whenever we receive a message of this type, try to remove the delegate
            // ReSharper disable once DelegateSubtraction
            kerbalEva.fsm.OnStateChange -= LmpOnStateChange;
            
            if (!kerbalEva.fsm.Started)
                kerbalEva.fsm?.StartFSM("Idle (Grounded)");

            var fsmState = FsmStates.FirstOrDefault(e => (e.GetValue(kerbalEva) as KFSMEvent)?.name == eventToRun);

            if (!(fsmState?.GetValue(kerbalEva) is KFSMEvent kfsmEventToRun)) return;

            kerbalEva.fsm.RunEvent(kfsmEventToRun);
        }

        /// <summary>
        /// This is the eva callback that sends the events to the server
        /// </summary>
        public void LmpOnStateChange(KFSMState prevState, KFSMState newState, KFSMEvent eventToRun)
        {
            var module = GetKerbalEvaModule(FlightGlobals.ActiveVessel) as KerbalEVA;
            if (module != null)
            {
                MessageSender.SendEvaData(FlightGlobals.ActiveVessel, newState.name, eventToRun.name, module.lastBoundStep);
                UpdateFsmStateInProtoVessel(FlightGlobals.ActiveVessel.protoVessel, newState.name, module.lastBoundStep);
            }
            else
            {
                MessageSender.SendEvaData(FlightGlobals.ActiveVessel, newState.name, eventToRun.name);
                UpdateFsmStateInProtoVessel(FlightGlobals.ActiveVessel.protoVessel, newState.name);
            }
        }

        /// <summary>
        /// Updates the fsm state in the given protovessel
        /// </summary>
        public void UpdateFsmStateInProtoVessel(ProtoVessel protoVessel, string state, float lastBoundStep = float.NaN)
        {
            var protoModule = GetKerbalEvaProtoModule(protoVessel);

            protoModule?.moduleValues.SetValue("state", state);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (lastBoundStep != float.NaN)
                protoModule?.moduleValues.SetValue("lastBoundStep", lastBoundStep);
        }
        
        private static ProtoPartModuleSnapshot GetKerbalEvaProtoModule(ProtoVessel protoVessel)
        {
            if (protoVessel == null) return null;

            var partSnapshot = VesselCommon.FindProtoPartInProtovessel(protoVessel, "kerbalEVA");
            if (partSnapshot == null) return null;

            return VesselCommon.FindProtoPartModuleInProtoPart(partSnapshot, "KerbalEVA");
        }

        private static PartModule GetKerbalEvaModule(Vessel vessel)
        {
            if (vessel == null) return null;

            var partSnapshot = VesselCommon.FindPartInVessel(vessel, "kerbalEVA");
            if (partSnapshot == null) return null;

            return VesselCommon.FindModuleInPart(partSnapshot, "KerbalEVA");
        }
    }
}
