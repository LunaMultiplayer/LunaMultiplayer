using LunaClient.Base;
using LunaClient.VesselUtilities;
using UnityEngine;

namespace LunaClient.Systems.VesselResourceSys
{
    public class VesselResourceSystem : MessageSystem<VesselResourceSystem, VesselResourceMessageSender, VesselResourceMessageHandler>
    {
        #region Fields & properties

        public bool ResourceSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                                 FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                                 !FlightGlobals.ActiveVessel.packed && FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;
        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselResourceSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, SendVesselResources));
        }

        #endregion

        #region Update routines

        private void SendVesselResources()
        {
            if (ResourceSystemReady && !VesselCommon.IsSpectating)
            {
                MessageSender.SendVesselResources(FlightGlobals.ActiveVessel);
            }
        }

        #endregion
    }
}
