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
        /// Update the vessel files with flightstate data max at a 2,5 seconds interval
        /// </summary>
        private const int FileFlightStateUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastFlightStateUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        /// <summary>
        /// We received a flight state information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WriteFlightstateDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselFlightStateMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastFlightStateUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FileFlightStateUpdateIntervalMs)
            {
                LastFlightStateUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                    {
                        if (!VesselStoreSystem.CurrentVessels.TryGetValue(msgData.VesselId, out var vessel)) return;

                        vessel.CtrlState.UpdateValue("pitch", msgData.Pitch.ToString(CultureInfo.InvariantCulture));
                        vessel.CtrlState.UpdateValue("yaw", msgData.Yaw.ToString(CultureInfo.InvariantCulture));
                        vessel.CtrlState.UpdateValue("roll", msgData.Roll.ToString(CultureInfo.InvariantCulture));
                        vessel.CtrlState.UpdateValue("trimPitch", msgData.PitchTrim.ToString(CultureInfo.InvariantCulture));
                        vessel.CtrlState.UpdateValue("trimYaw", msgData.YawTrim.ToString(CultureInfo.InvariantCulture));
                        vessel.CtrlState.UpdateValue("trimRoll", msgData.RollTrim.ToString(CultureInfo.InvariantCulture));
                        vessel.CtrlState.UpdateValue("mainThrottle", msgData.MainThrottle.ToString(CultureInfo.InvariantCulture));
                    }
                });
            }
        }
    }
}
