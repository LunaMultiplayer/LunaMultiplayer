using LunaClient.Base;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LunaClient.Systems.VesselFairingsSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselFairingsSystem : MessageSystem<VesselFairingsSystem, VesselFairingsMessageSender, VesselFairingsMessageHandler>
    {
        #region Fields & properties

        public bool FairingSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT && Time.timeSinceLevelLoad > 3f;

        private List<Vessel> SecondaryVesselsToUpdate { get; } = new List<Vessel>();

        private static readonly FieldInfo FsmField = typeof(ModuleProceduralFairing).GetField("fsm", BindingFlags.Instance | BindingFlags.NonPublic);

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselFairingsSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, SendVesselFairings));
            SetupRoutine(new RoutineDefinition(5000, RoutineExecution.Update, SendSecondaryVesselFairings));
        }
        #endregion

        #region Update routines

        private void SendVesselFairings()
        {
            if (FairingSystemReady && !VesselCommon.IsSpectating)
            {
                CheckAndSendFairingChanges(FlightGlobals.ActiveVessel);
            }
        }

        private void SendSecondaryVesselFairings()
        {
            if (FairingSystemReady && !VesselCommon.IsSpectating)
            {
                SecondaryVesselsToUpdate.Clear();
                SecondaryVesselsToUpdate.AddRange(VesselCommon.GetSecondaryVessels());

                for (var i = 0; i < SecondaryVesselsToUpdate.Count; i++)
                {
                    CheckAndSendFairingChanges(SecondaryVesselsToUpdate[i]);
                }
            }
        }

        #endregion

        private void CheckAndSendFairingChanges(Vessel vessel)
        {
            if (vessel == null) return;

            var proceduralFairingModules = vessel.FindPartModulesImplementing<ModuleProceduralFairing>();
            for (var i = 0; i < proceduralFairingModules.Count; i++)
            {
                var module = proceduralFairingModules[i];
                var storeValue = GetFairingStateFromStore(vessel.id, module.part.flightID);

                if (storeValue == "st_idle" && FsmField?.GetValue(module) is KerbalFSM fsm && fsm.currentStateName == "st_flight_deployed")
                {
                    LunaLog.Log($"Detected fairings deployed! Part FlightID: {proceduralFairingModules[i].part.flightID}");
                    MessageSender.SendVesselFairingDeployed(vessel, proceduralFairingModules[i].part.flightID);
                    UpdateFairingsValuesInProtoVessel(vessel.protoVessel, module.part.flightID);
                }
            }
        }

        public string GetFairingStateFromStore(Guid vesselId, uint partFlightId)
        {
            if (VesselsProtoStore.AllPlayerVessels.TryGetValue(vesselId, out var vesselProtoUpd))
            {
                var protoVessel = vesselProtoUpd.ProtoVessel;

                var protoPart = VesselCommon.FindProtoPartInProtovessel(protoVessel, partFlightId);
                if (protoPart == null) return null;

                var protoModule = VesselCommon.FindProtoPartModuleInProtoPart(protoPart, "ModuleProceduralFairing");

                return protoModule?.moduleValues.GetValue("fsm");
            }

            return null;
        }

        public void UpdateFairingsValuesInProtoVessel(ProtoVessel protoVessel, uint partFlightId)
        {
            if (protoVessel == null) return;

            var protoPart = VesselCommon.FindProtoPartInProtovessel(protoVessel, partFlightId);
            if (protoPart == null) return;

            var protoModule = VesselCommon.FindProtoPartModuleInProtoPart(protoPart, "ModuleProceduralFairing");
            if (protoModule == null) return;

            protoModule.moduleValues.SetValue("fsm", "st_flight_deployed");
            protoModule.moduleValues.RemoveNodesStartWith("XSECTION");
        }
    }
}
