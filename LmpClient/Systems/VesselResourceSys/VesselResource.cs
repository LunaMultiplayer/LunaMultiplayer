using System;
using LmpClient;
using LmpClient.Extensions;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;

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
            var vessel = FlightGlobals.FindVessel(VesselId);
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
                if (Resources == null || i >= Resources.Length || Resources[i] == null)
                {
                    LunaLog.LogWarning(
                        $"[LMP]: Skipping ProtoPart resource write due to failure to match (vessel {VesselId}, index {i}, reason: invalid resource entry).");
                    continue;
                }

                var partSnapshot = vessel.protoVessel.GetProtoPart(Resources[i].PartFlightId);

                //The part may have been removed (e.g. destroyed by a collision) while this update was in flight, skip it
                if (partSnapshot == null)
                {
                    LunaLog.LogWarning(
                        $"[LMP]: Skipping ProtoPart resource write due to failure to match (vessel {VesselId}, part {Resources[i].PartFlightId}, resource '{Resources[i].ResourceName}', reason: proto part not found).");
                    continue;
                }

                var resourceSnapshot = partSnapshot.FindResourceInProtoPart(Resources[i].ResourceName);
                if (resourceSnapshot == null)
                {
                    LunaLog.LogWarning(
                        $"[LMP]: Skipping ProtoPart resource write due to failure to match (vessel {VesselId}, part {Resources[i].PartFlightId}, resource '{Resources[i].ResourceName}', reason: proto resource not found).");
                    continue;
                }

                resourceSnapshot.amount = Resources[i].Amount;
                resourceSnapshot.flowState = Resources[i].FlowState;

                //Using "resourceSnapshot.resourceRef" sometimes returns null so we also try to get the resource from the part...
                if (resourceSnapshot.resourceRef == null)
                {
                    if (partSnapshot.partRef != null)
                    {
                        var foundResource = partSnapshot.partRef.FindResource(resourceSnapshot.resourceName);

                        //The resource may no longer exist on the part (e.g. the part is being destroyed by a collision)
                        if (foundResource != null)
                        {
                            foundResource.amount = Resources[i].Amount;
                            foundResource.flowState = Resources[i].FlowState;
                        }
                        else
                        {
                            LunaLog.LogWarning($"[LMP]: Skipping ProtoPart resource write due to failure to match (vessel {VesselId}, part {Resources[i].PartFlightId}, resource '{resourceSnapshot.resourceName}', reason: live resource not found on part).");
                        }
                    }
                }
                else
                {
                    resourceSnapshot.resourceRef.amount = Resources[i].Amount;
                    resourceSnapshot.resourceRef.flowState = Resources[i].FlowState;
                }
            }
        }
    }
}
