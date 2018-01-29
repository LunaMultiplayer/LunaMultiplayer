using LunaClient.Base;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace LunaClient.VesselStore
{
    /// <summary>
    /// This class holds a dictionary with all the vessel definitions that we received either when 2 vessels are docked or some vessel part changes or 
    /// a vessel gets a change. It's a way of unifying the data-storage of VesselChangeSystem and VesselProtoSystem and also be able to update a 
    /// vessel definition when a docking happens or to acces this dictionary from the VesselRemoveSystem
    /// </summary>
    public class VesselsProtoStore
    {
        public static ConcurrentDictionary<Guid, VesselProtoUpdate> AllPlayerVessels { get; } =
            new ConcurrentDictionary<Guid, VesselProtoUpdate>();

        /// <summary>
        /// In this method we get the new vessel data and set it to the dictionary of all the player vessels.
        /// </summary>
        public static void HandleVesselProtoData(byte[] vesselData, int numBytes, Guid vesselId, bool runSyncronously, int vesselSituation = int.MinValue)
        {
            if (runSyncronously)
                HandleData();
            else
                SystemBase.TaskFactory.StartNew(HandleData);

            void HandleData()
            {
                if (AllPlayerVessels.TryGetValue(vesselId, out var vesselUpdate))
                {
                    vesselUpdate.Update(vesselData, numBytes, vesselId, vesselSituation);
                }
                else
                {
                    AllPlayerVessels.TryAdd(vesselId, new VesselProtoUpdate(vesselData, numBytes, vesselId));
                }
            }
        }

        /// <summary>
        /// Clears the whole system
        /// </summary>
        public static void ClearSystem()
        {
            AllPlayerVessels.Clear();
        }

        /// <summary>
        /// Removes a vessel from the proto system. Bar in mind that if we receive a protovessel msg after this method is called it will be reloaded
        /// </summary>
        public static void RemoveVessel(Guid vesselId)
        {
            AllPlayerVessels.TryRemove(vesselId, out var _);
        }

        /// <summary>
        /// Check in the store if there's a vessel with that part in it's protovessel. If so it returns that vessel
        /// </summary>
        public static Vessel GetVesselByPartId(uint flightId)
        {
            var keys = AllPlayerVessels.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                if (AllPlayerVessels.TryGetValue(keys[i], out var vesselProtoUpd))
                {
                    if (vesselProtoUpd.VesselParts.ContainsKey(flightId))
                        return vesselProtoUpd.Vessel;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a vessel manually to the dictionary. Use this to add your own spawned vessels.
        /// </summary>
        public static void AddVesselToDictionary(Vessel vessel)
        {
            var ownVesselData = VesselSerializer.SerializeVessel(vessel.protoVessel);
            if (ownVesselData.Length > 0)
            {
                var newProtoUpdate = new VesselProtoUpdate(ownVesselData, ownVesselData.Length, FlightGlobals.ActiveVessel.id);
                AllPlayerVessels.AddOrUpdate(FlightGlobals.ActiveVessel.id, newProtoUpdate, (key, existingVal) => newProtoUpdate);
            }
        }
    }
}
