using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.ModuleStore;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    public class VesselPartModuleSyncMessageHandler : SubSystem<VesselPartModuleSyncSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public static readonly Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>> LastUpdatedDictionary =
            new Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPartSyncMsgData msgData) || !System.PartSyncSystemReady) return;

            //Vessel might exist in the store but not in game (if the vessel is in safety bubble for example)
            VesselsProtoStore.UpdateVesselProtoPartModules(msgData);

            var vessel = FlightGlobals.FindVessel(msgData.VesselId);
            if (vessel == null) return;

            UpdateVesselValues(vessel.protoVessel, msgData);
        }

        private static void UpdateVesselValues(ProtoVessel protoVessel, VesselPartSyncMsgData msgData)
        {
            if (protoVessel == null) return;

            var part = VesselCommon.FindProtoPartInProtovessel(protoVessel, msgData.PartFlightId);
            if (part != null)
            {
                var module = VesselCommon.FindProtoPartModuleInProtoPart(part, msgData.ModuleName);
                if (module != null)
                {
                    module.moduleValues.SetValue(msgData.FieldName, msgData.Value);
                    UpdateVesselModuleIfNeeded(protoVessel.vesselID, part.flightID, msgData.FieldName, module, part);
                }
            }
        }

        private static void UpdateVesselModuleIfNeeded(Guid vesselId, uint partFlightId, string fieldName, ProtoPartModuleSnapshot module, ProtoPartSnapshot part)
        {
            if (module.moduleRef == null) return;

            switch (SkipUpdateModule(vesselId, partFlightId, module.moduleName, fieldName))
            {
                case CustomizationResult.TooEarly:
                case CustomizationResult.IgnoreSend:
                    break;
                case CustomizationResult.Ok:
                    module.moduleRef?.Load(module.moduleValues);
                    module.moduleRef?.OnAwake();
                    module.moduleRef?.OnLoad(module.moduleValues);
                    module.moduleRef?.OnStart(part.partRef.GetModuleStartState());
                    break;
            }
        }

        private static CustomizationResult SkipUpdateModule(Guid vesselId, uint partFlightId, string moduleName, string fieldName)
        {
            var fieldCustomization = FieldModuleStore.GetCustomFieldDefinition(moduleName, fieldName);
            if (fieldCustomization.IgnoreSend) return CustomizationResult.IgnoreSend;

            try
            {
                if (!LastUpdatedDictionary[vesselId][partFlightId][moduleName][fieldName].IntervalIsOk()) return CustomizationResult.TooEarly;

                LastUpdatedDictionary[vesselId][partFlightId][moduleName][fieldName].Update();
                return CustomizationResult.Ok;
            }
            catch (Exception)
            {
                if (!LastUpdatedDictionary.ContainsKey(vesselId))
                    LastUpdatedDictionary.Add(vesselId, new Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>());
                if (!LastUpdatedDictionary[vesselId].ContainsKey(partFlightId))
                    LastUpdatedDictionary[vesselId].Add(partFlightId, new Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>());
                if (!LastUpdatedDictionary[vesselId][partFlightId].ContainsKey(moduleName))
                    LastUpdatedDictionary[vesselId][partFlightId].Add(moduleName, new Dictionary<string, PartSyncUpdateEntry>());
                if (!LastUpdatedDictionary[vesselId][partFlightId][moduleName].ContainsKey(fieldName))
                    LastUpdatedDictionary[vesselId][partFlightId][moduleName].Add(fieldName, new PartSyncUpdateEntry(fieldCustomization.IntervalCheckChangesMs));
            }

            return CustomizationResult.Ok;
        }
    }
}
