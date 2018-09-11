using LunaCommon.Time;
using System;
using System.Collections.Generic;

namespace LunaClient.Systems.VesselPartModuleFieldSyncSys
{
    public class CustomizationsHandler
    {
        public static readonly Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, DateTime>>>> LastSendUpdatedDictionary =
            new Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, DateTime>>>>();

        public static bool TimeToSendFieldModule(Guid vesselId, uint partFlightId, string moduleName, string fieldName)
        {
            AddValuesToSendDictionaryIfMissing(vesselId, partFlightId, moduleName, fieldName);

            if (LunaComputerTime.UtcNow - LastSendUpdatedDictionary[vesselId][partFlightId][moduleName][fieldName] < TimeSpan.FromMilliseconds(2500))
                return false;

            LastSendUpdatedDictionary[vesselId][partFlightId][moduleName][fieldName] = LunaComputerTime.UtcNow;
            return true;
        }
        
        private static void AddValuesToSendDictionaryIfMissing(Guid vesselId, uint partFlightId, string moduleName, string fieldName)
        {
            if (!LastSendUpdatedDictionary.ContainsKey(vesselId))
                LastSendUpdatedDictionary.Add(vesselId, new Dictionary<uint, Dictionary<string, Dictionary<string, DateTime>>>());
            if (!LastSendUpdatedDictionary[vesselId].ContainsKey(partFlightId))
                LastSendUpdatedDictionary[vesselId].Add(partFlightId, new Dictionary<string, Dictionary<string, DateTime>>());
            if (!LastSendUpdatedDictionary[vesselId][partFlightId].ContainsKey(moduleName))
                LastSendUpdatedDictionary[vesselId][partFlightId].Add(moduleName, new Dictionary<string, DateTime>());
            if (!LastSendUpdatedDictionary[vesselId][partFlightId][moduleName].ContainsKey(fieldName))
                LastSendUpdatedDictionary[vesselId][partFlightId][moduleName].Add(fieldName, DateTime.MinValue);
        }
    }
}
