using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.ModuleStore;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    public class VesselPartModuleSyncMessageHandler : SubSystem<VesselPartModuleSyncSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPartSyncMsgData msgData) || !System.PartSyncSystemReady) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;

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
                    UpdateVesselModuleIfNeeded(protoVessel.vesselID, part.flightID, msgData, module, part);
                }
            }
        }

        private static void UpdateVesselModuleIfNeeded(Guid vesselId, uint partFlightId, VesselPartSyncMsgData msgData, ProtoPartModuleSnapshot module, ProtoPartSnapshot part)
        {
            if (module.moduleRef == null) return;

            switch (CustomizationsHandler.SkipModule(vesselId, partFlightId, msgData.BaseModuleName, msgData.FieldName, true, out var customization))
            {
                case CustomizationResult.TooEarly:
                case CustomizationResult.Ignore:
                    break;
                case CustomizationResult.Ok:
                    if (customization.IgnoreSpectating && FlightGlobals.ActiveVessel?.id == vesselId) break;

                    if (customization.SetValueInModule)
                    {
                        if (FieldModuleStore.ModuleFieldsDictionary.TryGetValue(msgData.BaseModuleName, out var moduleDef))
                        {
                            if (moduleDef.PersistentModuleField.TryGetValue(msgData.FieldName, out var fieldDef))
                            {
                                var convertedVal = Convert.ChangeType(msgData.Value, fieldDef.FieldType);
                                if (convertedVal != null) fieldDef.SetValue(module.moduleRef, convertedVal);
                            }
                        }
                    }

                    if (customization.CallLoad)
                        module.moduleRef.Load(module.moduleValues);
                    if (customization.CallOnAwake)
                        module.moduleRef.OnAwake();
                    if (customization.CallOnLoad)
                        module.moduleRef.OnLoad(module.moduleValues);
                    if (customization.CallOnStart)
                        module.moduleRef.OnStart(part.partRef.GetModuleStartState());
                    break;
            }
        }
    }
}
