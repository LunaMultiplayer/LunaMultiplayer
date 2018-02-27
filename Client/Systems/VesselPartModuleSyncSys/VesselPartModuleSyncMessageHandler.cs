using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    public class VesselPartModuleSyncMessageHandler : SubSystem<VesselPartModuleSyncSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

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

            switch (CustomizationsHandler.SkipModule(vesselId, partFlightId, module.moduleName, fieldName, true, out var customization))
            {
                case CustomizationResult.TooEarly:
                case CustomizationResult.Ignore:
                    break;
                case CustomizationResult.Ok:
                    if (module.moduleName == "ModuleProceduralFairing")
                        (module.moduleRef as ModuleProceduralFairing)?.DeployFairing();
                    else
                    {
                        module.moduleRef?.Load(module.moduleValues);
                        if (customization.CallOnAwake)
                            module.moduleRef?.OnAwake();
                        if (customization.CallOnLoad)
                            module.moduleRef?.OnLoad(module.moduleValues);
                        if (customization.CallOnStart)
                            module.moduleRef?.OnStart(part.partRef.GetModuleStartState());
                    }
                    break;
            }
        }
    }
}
