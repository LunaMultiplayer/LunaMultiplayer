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
        /// Raw updates a vessel in the dictionary and takes care of the locking in case we received another vessel message type
        /// </summary>
        public static void RawConfigNodeInsertOrUpdate(Guid vesselId, string vesselDataInConfigNodeFormat)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd(vesselId, new object()))
                {
                    var vessel = new Structures.Vessel(vesselDataInConfigNodeFormat);

                    if (GeneralSettings.SettingsStore.ModControl)
                    {
                        var vesselParts = vessel.Parts.GetAllValues().Select(p=> p.Fields.GetSingle("name").Value);
                        var bannedParts = vesselParts.Except(ModFileSystem.ModControl.AllowedParts);
                        if (bannedParts.Any())
                        {
                            LunaLog.Warning($"Received a vessel with BANNED parts! {vesselId}");
                            return;
                        }
                    }

                    VesselStoreSystem.CurrentVessels.AddOrUpdate(vesselId, vessel, (key, existingVal) => vessel);
                }
            });
        }
    }
}
