using LunaClient.ModuleStore;
using System;
using System.Collections.Generic;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    public class CustomizationsHandler
    {

        public static readonly Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>> LastSendUpdatedDictionary =
            new Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>>();

        private static readonly Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>> LastReceiveUpdatedDictionary =
            new Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>>();

        public static CustomizationResult SkipModule(Guid vesselId, uint partFlightId, string moduleName, string fieldName, bool receive)
        {
            var fieldCustomization = FieldModuleStore.GetCustomFieldDefinition(moduleName, fieldName);
            if (fieldCustomization.Ignore) return CustomizationResult.Ignore;
            if (receive)
            { 
                try
                {
                    if (!LastReceiveUpdatedDictionary[vesselId][partFlightId][moduleName][fieldName].IntervalIsOk()) return CustomizationResult.TooEarly;

                    LastReceiveUpdatedDictionary[vesselId][partFlightId][moduleName][fieldName].Update();
                    return CustomizationResult.Ok;
                }
                catch (Exception)
                {
                    if (!LastReceiveUpdatedDictionary.ContainsKey(vesselId))
                        LastReceiveUpdatedDictionary.Add(vesselId, new Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>());
                    if (!LastReceiveUpdatedDictionary[vesselId].ContainsKey(partFlightId))
                        LastReceiveUpdatedDictionary[vesselId].Add(partFlightId, new Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>());
                    if (!LastReceiveUpdatedDictionary[vesselId][partFlightId].ContainsKey(moduleName))
                        LastReceiveUpdatedDictionary[vesselId][partFlightId].Add(moduleName, new Dictionary<string, PartSyncUpdateEntry>());
                    if (!LastReceiveUpdatedDictionary[vesselId][partFlightId][moduleName].ContainsKey(fieldName))
                        LastReceiveUpdatedDictionary[vesselId][partFlightId][moduleName].Add(fieldName, new PartSyncUpdateEntry(fieldCustomization.Interval));
                }

                return CustomizationResult.Ok;
            }

            try
            {
                if (!LastSendUpdatedDictionary[vesselId][partFlightId][moduleName][fieldName].IntervalIsOk()) return CustomizationResult.TooEarly;

                LastSendUpdatedDictionary[vesselId][partFlightId][moduleName][fieldName].Update();
                return CustomizationResult.Ok;
            }
            catch (Exception)
            {
                if (!LastSendUpdatedDictionary.ContainsKey(vesselId))
                    LastSendUpdatedDictionary.Add(vesselId, new Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>());
                if (!LastSendUpdatedDictionary[vesselId].ContainsKey(partFlightId))
                    LastSendUpdatedDictionary[vesselId].Add(partFlightId, new Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>());
                if (!LastSendUpdatedDictionary[vesselId][partFlightId].ContainsKey(moduleName))
                    LastSendUpdatedDictionary[vesselId][partFlightId].Add(moduleName, new Dictionary<string, PartSyncUpdateEntry>());
                if (!LastSendUpdatedDictionary[vesselId][partFlightId][moduleName].ContainsKey(fieldName))
                    LastSendUpdatedDictionary[vesselId][partFlightId][moduleName].Add(fieldName, new PartSyncUpdateEntry(fieldCustomization.Interval));
            }

            return CustomizationResult.Ok;
        }
    }
}
