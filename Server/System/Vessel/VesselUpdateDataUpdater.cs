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
        /// Update the vessel files with update data max at a 2,5 seconds interval
        /// </summary>
        private const int FileUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        /// <summary>
        /// We received a update information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WriteUpdateDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselUpdateMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FileUpdateIntervalMs)
            {
                LastUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                    {
                        if (!VesselStoreSystem.CurrentVessels.TryGetValue(msgData.VesselId, out var vessel)) return;

                        vessel.Fields.Update("name", msgData.Name);
                        vessel.Fields.Update("type", msgData.Type);
                        vessel.Fields.Update("distanceTraveled", msgData.DistanceTraveled.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("sit", msgData.Situation);
                        vessel.Fields.Update("landed", msgData.Landed.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("landedAt", msgData.LandedAt);
                        vessel.Fields.Update("displaylandedAt", msgData.DisplayLandedAt);
                        vessel.Fields.Update("splashed", msgData.Splashed.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("met", msgData.MissionTime.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("lct", msgData.LaunchTime.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("lastUT", msgData.LastUt.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("prst", msgData.Persistent.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("ref", msgData.RefTransformId.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("cln", msgData.AutoClean.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("clnRsn", msgData.AutoCleanReason);
                        vessel.Fields.Update("ctrl", msgData.WasControllable.ToString(CultureInfo.InvariantCulture));
                        vessel.Fields.Update("stg", msgData.Stage.ToString(CultureInfo.InvariantCulture));

                        //NEVER! patch the CoM in the protovessel as then it will be drawn with incorrect CommNet lines!
                        //vessel.Fields.Update("CoM", $"{msgData.Com[0].ToString(CultureInfo.InvariantCulture)}," +
                        //                                $"{msgData.Com[1].ToString(CultureInfo.InvariantCulture)}," +
                        //                                $"{msgData.Com[2].ToString(CultureInfo.InvariantCulture)}");
                    }
                });
            }
        }
    }
}
