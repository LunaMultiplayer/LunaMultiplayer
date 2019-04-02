using Harmony;
using LmpClient.Extensions;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Vessel;
using System;
using System.Reflection;

namespace LmpClient.Systems.VesselCoupleSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselCouple
    {
        private static readonly MethodInfo GrappleMethod = typeof(ModuleGrappleNode).GetMethod("Grapple", AccessTools.all);
        private static readonly FieldInfo KerbalSeatField = typeof(KerbalEVA).GetField("kerbalSeat", AccessTools.all);

        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;
        public Guid CoupledVesselId;
        public uint PartFlightId;
        public uint CoupledPartFlightId;
        public CoupleTrigger Trigger;

        private static bool _activeVesselIsWeakVessel;
        private static bool _activeVesselIsDominantVessel;

        private static Vessel _dominantVessel;
        private static Vessel _weakVessel;

        #endregion

        public bool ProcessCouple()
        {
            _activeVesselIsWeakVessel = FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == CoupledVesselId;
            _activeVesselIsDominantVessel = FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == VesselId;

            var coupleResult = ProcessCoupleInternal(VesselId, CoupledVesselId, PartFlightId, CoupledPartFlightId, Trigger);
            AfterCouplingEvent();

            if (!coupleResult)
                VesselRemoveSystem.Singleton.KillVessel(CoupledVesselId, false, "Killing coupled vessel during a undetected coupling");

            return coupleResult;
        }

        public static bool ProcessCouple(VesselCoupleMsgData msgData)
        {
            _activeVesselIsWeakVessel = FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == msgData.CoupledVesselId;
            _activeVesselIsDominantVessel = FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == msgData.VesselId;

            var coupleResult = ProcessCoupleInternal(msgData.VesselId, msgData.CoupledVesselId, msgData.PartFlightId, msgData.CoupledPartFlightId, (CoupleTrigger)msgData.Trigger);
            AfterCouplingEvent();

            if (!coupleResult)
                VesselRemoveSystem.Singleton.KillVessel(msgData.CoupledVesselId, false, "Killing coupled vessel during a undetected coupling");

            return coupleResult;
        }

        private static bool ProcessCoupleInternal(Guid vesselId, Guid coupledVesselId, uint partFlightId, uint coupledPartFlightId, CoupleTrigger trigger)
        {
            if (!VesselCommon.DoVesselChecks(vesselId))
                return false;

            //If the coupling is against our OWN vessel we must FORCE the loading
            var forceLoad = FlightGlobals.ActiveVessel && (FlightGlobals.ActiveVessel.id == vesselId || FlightGlobals.ActiveVessel.id == coupledVesselId);

            _dominantVessel = FlightGlobals.FindVessel(vesselId);
            if (_dominantVessel == null) return false;
            if (!_dominantVessel.loaded && forceLoad) _dominantVessel.Load();

            _weakVessel = FlightGlobals.FindVessel(coupledVesselId);
            if (_weakVessel == null) return false;
            if (!_weakVessel.loaded && forceLoad) _weakVessel.Load();

            var protoPart = _dominantVessel.protoVessel.GetProtoPart(partFlightId);
            var coupledProtoPart = _weakVessel.protoVessel.GetProtoPart(coupledPartFlightId);
            if (protoPart != null && coupledProtoPart != null)
            {
                if (protoPart.partRef && coupledProtoPart.partRef)
                {
                    VesselCoupleSystem.Singleton.IgnoreEvents = true;

                    //Remember! The weak vessel must couple with the DOMINANT vessel and not the other way around!

                    switch (trigger)
                    {
                        case CoupleTrigger.DockingNode:
                            var weakDockingNode = coupledProtoPart.partRef.FindModuleImplementing<ModuleDockingNode>();
                            if (weakDockingNode)
                            {
                                var dominantDockingNode = protoPart.partRef.FindModuleImplementing<ModuleDockingNode>();
                                if (dominantDockingNode)
                                {
                                    weakDockingNode.DockToVessel(dominantDockingNode);
                                }
                            }
                            break;
                        case CoupleTrigger.GrappleNode:
                            var grappleModule = coupledProtoPart.partRef.FindModuleImplementing<ModuleGrappleNode>();
                            if (grappleModule)
                            {
                                GrappleMethod.Invoke(grappleModule, new object[] { coupledProtoPart, protoPart });
                            }
                            break;
                        case CoupleTrigger.Kerbal:
                            var kerbalEva = coupledProtoPart.partRef.FindModuleImplementing<KerbalEVA>();
                            if (kerbalEva)
                            {
                                var seat = KerbalSeatField.GetValue(kerbalEva) as KerbalSeat;
                                if (seat)
                                {
                                    kerbalEva.fsm.RunEvent(kerbalEva.On_seatBoard);
                                }
                            }
                            break;
                        case CoupleTrigger.Other:
                            coupledProtoPart.partRef.Couple(protoPart.partRef);
                            break;
                    }
                    VesselCoupleSystem.Singleton.IgnoreEvents = false;

                    return true;
                }
            }

            return false;
        }

        private static void AfterCouplingEvent()
        {
            if (_activeVesselIsWeakVessel)
            {
                if (_dominantVessel)
                {
                    FlightGlobals.ForceSetActiveVessel(_dominantVessel);
                    FlightInputHandler.SetNeutralControls();
                }
            }

            if (_activeVesselIsDominantVessel)
            {
                _dominantVessel.MakeActive();
                FlightInputHandler.SetNeutralControls();
            }
        }
    }
}
