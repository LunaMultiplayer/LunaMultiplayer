using LmpCommon.Message.Data.Vessel;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;

namespace Server.System.Vessel
{
    /// <summary>
    /// We try to avoid working with protovessels as much as possible as they can be huge files.
    /// This class patches the vessel file with the information messages we receive about a position and other vessel properties.
    /// This way we send the whole vessel definition only when there are parts that have changed 
    /// </summary>
    public partial class VesselDataUpdater
    {        
        /// <summary>
        /// Update the vessel files with resource data max at a 2,5 seconds interval
        /// </summary>
        private const int FileResourcesUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastResourcesUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();


        /// <summary>
        /// We received a resource information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later received an update vesselproto
        /// </summary>
        public static void WriteResourceDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselResourceMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastResourcesUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FileResourcesUpdateIntervalMs)
            {
                LastResourcesUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                    {
                        if (!VesselStoreSystem.CurrentVessels.TryGetValue(msgData.VesselId, out var vessel)) return;

                        foreach (var resource in msgData.Resources)
                        {
                            var part = vessel.GetPart(resource.PartFlightId);
                            if (part != null)
                            {
                                var resourceNode = part.Resources.GetSingle(resource.ResourceName).Value;
                                if (resourceNode != null)
                                {
                                    resourceNode.UpdateValue("amount", resource.Amount.ToString(CultureInfo.InvariantCulture));
                                    resourceNode.UpdateValue("flowState", resource.FlowState.ToString(CultureInfo.InvariantCulture));
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
