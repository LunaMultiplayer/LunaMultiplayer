using LunaCommon.FastMember;
using System;
using System.Collections.Generic;

namespace LunaClient.Systems.VesselPartSyncFieldSys
{
    /// <summary>
    /// This class allows a fast direct access to the part modules of a vessel without having to do shitty nested loops
    /// </summary>
    public class VesselPartModuleAccess
    {
        public static Dictionary<Guid, Dictionary<uint, Dictionary<string, ObjectAccessor>>> AccessorDictionary = 
            new Dictionary<Guid, Dictionary<uint, Dictionary<string, ObjectAccessor>>>();

        /// <summary>
        /// Gets the field value of a part module
        /// </summary>
        public static object GetPartModuleFieldValue(Guid vesselId, uint partId, string partModuleName, string field)
        {
            if (AccessorDictionary.TryGetValue(vesselId, out var partsDictionary))
            {
                if (partsDictionary.TryGetValue(partId, out var modulesDictionary))
                {
                    if (modulesDictionary.TryGetValue(partModuleName, out var fieldAccessor))
                    {
                        if (fieldAccessor.Target != null)
                        {
                            return fieldAccessor[field];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the field value of a part module
        /// </summary>
        public static bool SetPartModuleFieldValue(Guid vesselId, uint partId, string partModuleName, string field, object value)
        {
            if (AccessorDictionary.TryGetValue(vesselId, out var partsDictionary))
            {
                if (partsDictionary.TryGetValue(partId, out var modulesDictionary))
                {
                    if (modulesDictionary.TryGetValue(partModuleName, out var fieldAccessor))
                    {
                        if (fieldAccessor.Target != null)
                        {
                            fieldAccessor[field] = value;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a vessel to the system
        /// </summary>
        public static void AddVessel(Vessel vessel)
        {
            if (AccessorDictionary.ContainsKey(vessel.id))
                AccessorDictionary.Remove(vessel.id);

            AccessorDictionary.Add(vessel.id, new Dictionary<uint, Dictionary<string, ObjectAccessor>>());

            foreach (var part in vessel.parts)
            {
                AccessorDictionary[vessel.id].Add(part.flightID, new Dictionary<string, ObjectAccessor>());
                foreach (var partModule in part.Modules)
                {
                    AccessorDictionary[vessel.id][part.flightID].Add(partModule.moduleName, ObjectAccessor.Create(partModule));
                }
            }
        }

        /// <summary>
        /// Update the parts of a vessel
        /// </summary>
        public static void UpdateVessel(Vessel vessel)
        {
            if (AccessorDictionary.ContainsKey(vessel.id))
            {
                foreach (var part in vessel.parts)
                {
                    if (!AccessorDictionary[vessel.id].ContainsKey(part.flightID))
                    {
                        AccessorDictionary[vessel.id].Add(part.flightID, new Dictionary<string, ObjectAccessor>());
                        foreach (var partModule in part.Modules)
                        {
                            AccessorDictionary[vessel.id][part.flightID]
                                .Add(partModule.moduleName, ObjectAccessor.Create(partModule));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes a vessel from the system
        /// </summary>
        public static void RemoveVessel(Guid vesselId)
        {
            if (AccessorDictionary.ContainsKey(vesselId))
                AccessorDictionary.Remove(vesselId);
        }
    }
}
