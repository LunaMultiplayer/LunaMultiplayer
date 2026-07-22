using LunaConfigNode.CfgNode;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Collections.Concurrent;
using System.Linq;
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
        #region Semaphore

        /// <summary>
        /// To not overwrite our own data we use a lock
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, object> Semaphore = new ConcurrentDictionary<Guid, object>();

        #endregion

        /// <summary>
        /// Sets ORBIT IDENT from the reference body name when provided (e.g. from position or update messages).
        /// </summary>
        internal static void ApplyOrbitIdent(Classes.Vessel vessel, string bodyName)
        {
            if (string.IsNullOrEmpty(bodyName)) return;

            if (vessel.Orbit.Exists("IDENT"))
                vessel.Orbit.Update("IDENT", bodyName);
            else
                vessel.Orbit.Add(new CfgNodeValue<string, string>("IDENT", bodyName));
        }

        /// <summary>
        /// Raw updates a vessel in the dictionary and takes care of the locking in case we received another vessel message type
        /// </summary>
        public static void RawConfigNodeInsertOrUpdate(Guid vesselId, string vesselDataInConfigNodeFormat)
        {
            _ = Task.Run(() =>
            {
                // The insert is scheduled asynchronously, so a VesselRemove for the same vessel may arrive
                // while this task is queued. Re-check the kill list before touching the store so a delayed
                // insert cannot resurrect a vessel that has since been removed (e.g. revert-to-editor).
                if (VesselContext.RemovedVessels.ContainsKey(vesselId)) return;

                var vessel = new Classes.Vessel(vesselDataInConfigNodeFormat);
                if (GeneralSettings.SettingsStore.ModControl)
                {
                    var vesselParts = vessel.Parts.GetAllValues().Select(p => p.Fields.GetSingle("name").Value);
                    var bannedParts = vesselParts.Except(ModFileSystem.ModControl.AllowedParts);
                    if (bannedParts.Any())
                    {
                        LunaLog.Warning($"Received a vessel with BANNED parts! {vesselId}");
                        return;
                    }
                }
                lock (Semaphore.GetOrAdd(vesselId, new object()))
                {
                    // Re-check under the per-vessel lock to close the race against HandleVesselRemove,
                    // which now publishes to RemovedVessels before clearing the store entry.
                    if (VesselContext.RemovedVessels.ContainsKey(vesselId)) return;

                    VesselStoreSystem.CurrentVessels.AddOrUpdate(vesselId, vessel, (key, existingVal) => vessel);
                }
            });
        }
    }
}
