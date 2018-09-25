using LmpClient.Extensions;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using System;

namespace LmpClient.Systems.VesselResourceSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselResource
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;
        public int ResourcesCount;
        public VesselResourceInfo[] Resources = new VesselResourceInfo[0];

        #endregion

        public void ProcessVesselResource()
        {
            var vessel = FlightGlobals.fetch.LmpFindVessel(VesselId);
            if (vessel == null) return;

            if (!VesselCommon.DoVesselChecks(vessel.id))
                return;

            UpdateVesselFields(vessel);
        }
        
        private void UpdateVesselFields(Vessel vessel)
        {
            if (vessel.protoVessel == null) return;

            for (var i = 0; i < ResourcesCount; i++)
            {
                var partSnapshot = VesselCommon.FindProtoPartInProtovessel(vessel.protoVessel, Resources[i].PartFlightId);
                var resourceSnapshot = VesselCommon.FindResourceInProtoPart(partSnapshot, Resources[i].ResourceName);
                if (resourceSnapshot != null)
                {
                    resourceSnapshot.amount = Resources[i].Amount;
                    resourceSnapshot.flowState = Resources[i].FlowState;

                    if (resourceSnapshot.resourceRef == null) continue;

                    resourceSnapshot.resourceRef.amount = Resources[i].Amount;
                    resourceSnapshot.resourceRef.flowState = Resources[i].FlowState;
                }
            }
        }
    }
}
