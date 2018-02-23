using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.ModuleStore;
using LunaClient.Systems.SettingsSys;
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

        public static readonly Dictionary<Guid, PartSyncUpdateEntry> LastUpdatedDictionary = new Dictionary<Guid, PartSyncUpdateEntry>();

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
            if (SettingsSystem.CurrentSettings.Debug9) return;
            if (protoVessel == null) return;

            var part = VesselCommon.FindProtoPartInProtovessel(protoVessel, msgData.PartFlightId);
            if (part != null)
            {
                var module = VesselCommon.FindProtoPartModuleInProtoPart(part, msgData.ModuleName);
                if (module != null)
                {
                    module.moduleValues.SetValue(msgData.FieldName, msgData.Value);
                    UpdateVesselModuleIfNeeded(protoVessel.vesselID, msgData.FieldName, module, part);
                }
            }
        }

        private static void UpdateVesselModuleIfNeeded(Guid vesselId, string fieldName, ProtoPartModuleSnapshot module, ProtoPartSnapshot part)
        {
            if (SettingsSystem.CurrentSettings.Debug8) return;

            if (module.moduleRef == null) return;

            var fieldCustomization = FieldModuleStore.GetCustomFieldDefinition(module.moduleName, fieldName);

            if (fieldCustomization.IgnoreReceive) return;
            if (LastUpdatedDictionary.TryGetValue(vesselId, out var lastUpdateValue) && !lastUpdateValue.TimeToCheck) return;

            if (!LastUpdatedDictionary.ContainsKey(vesselId))
                LastUpdatedDictionary.Add(vesselId, new PartSyncUpdateEntry(fieldCustomization.IntervalApplyChangesMs));
            else
                LastUpdatedDictionary[vesselId].Update();

            //After we update the protovessel value try to update the vessel values...
            if (SettingsSystem.CurrentSettings.Debug7)
                module.moduleRef?.Load(module.moduleValues);
            if (SettingsSystem.CurrentSettings.Debug6)
                module.moduleRef?.OnAwake();
            if (SettingsSystem.CurrentSettings.Debug5)
                module.moduleRef?.OnLoad(module.moduleValues);
            if (SettingsSystem.CurrentSettings.Debug5)
                module.moduleRef?.OnStart(part.partRef.GetModuleStartState());
        }
    }
}
