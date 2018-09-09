using LunaClient.Base;
using LunaClient.Systems.TimeSyncer;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace LunaClient.Systems.VesselFairingsSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselFairingsSystem : MessageSystem<VesselFairingsSystem, VesselFairingsMessageSender, VesselFairingsMessageHandler>
    {
        #region Fields & properties

        public bool FairingSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT;
        
        private static readonly FieldInfo FsmField = typeof(ModuleProceduralFairing).GetField("fsm", BindingFlags.Instance | BindingFlags.NonPublic);

        public ConcurrentDictionary<Guid, VesselFairingQueue> VesselFairings { get; } = new ConcurrentDictionary<Guid, VesselFairingQueue>();

        #endregion

        #region Base overrides        

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselFairingsSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, SendVesselFairings));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, ProcessVesselFairings));
        }

        #endregion

        #region Update routines

        private void SendVesselFairings()
        {
            if (FairingSystemReady && !VesselCommon.IsSpectating)
            {
                var proceduralFairingModules = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleProceduralFairing>();
                for (var i = 0; i < proceduralFairingModules.Count; i++)
                {
                    var module = proceduralFairingModules[i];
                    var storeValue = module.snapshot.moduleValues.GetValue("fsm"); ;

                    if (storeValue == "st_idle" && FsmField?.GetValue(module) is KerbalFSM fsm && fsm.currentStateName == "st_flight_deployed")
                    {
                        LunaLog.Log($"Detected fairings deployed! Part FlightID: {proceduralFairingModules[i].part.flightID}");
                        MessageSender.SendVesselFairingDeployed(FlightGlobals.ActiveVessel, proceduralFairingModules[i].part.flightID);
                    }
                }
            }
        }

        private void ProcessVesselFairings()
        {
            foreach (var keyVal in VesselFairings)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncerSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessFairing();
                    keyVal.Value.Recycle(update);
                }
            }
        }

        #endregion
    }
}
