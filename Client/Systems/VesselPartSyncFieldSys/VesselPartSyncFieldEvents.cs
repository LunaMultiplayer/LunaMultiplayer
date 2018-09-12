using LunaClient.Base;
using LunaClient.ModuleStore;
using System;

namespace LunaClient.Systems.VesselPartSyncFieldSys
{
    public class VesselPartSyncFieldEvents : SubSystem<VesselPartSyncFieldSystem>
    {
        private static bool CallIsValid(PartModule module)
        {
            var vessel = module.vessel;
            if (vessel == null || !vessel.loaded || vessel.protoVessel == null)
                return false;

            var part = module.part;
            if (part == null)
                return false;

            //The vessel is immortal so we are sure that it's not ours
            if (float.IsPositiveInfinity(part.crashTolerance))
                return false;

            return true;
        }


        public void PartModuleFieldChanged(PartModule module, string fieldName)
        {
            if (!CallIsValid(module))
                return;

            var fieldVal = VesselPartModuleAccess.GetPartModuleFieldValue(module.vessel.id, module.part.flightID, module.moduleName, fieldName);
            if (fieldVal == null) return;

            if (FieldModuleStore.InheritanceTypeChain.TryGetValue(module.moduleName, out var inheritChain))
            {
                foreach (var baseModuleName in inheritChain)
                {
                    if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(baseModuleName, out var customization))
                    {
                        customization.GetCustomizationForField(fieldName);
                    }
                }
            }

            LunaLog.Log($"Part field {fieldName} changed in module {module.moduleName} from part {module.part.flightID}.");
            System.MessageSender.SendVesselPartSyncFieldMsg(module.vessel.id, module.part.flightID, module.moduleName, fieldName, fieldVal.ToString());
        }

        /// <summary>
        /// Sends our vessel just when we start the flight
        /// </summary>
        public void FlightReady()
        {
            if (FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.id == Guid.Empty)
                return;

            VesselPartModuleAccess.AddVessel(FlightGlobals.ActiveVessel);
        }

        public void VesselLoaded(Vessel vessel)
        {
            VesselPartModuleAccess.AddVessel(vessel);
        }

        public void VesselPartCountChanged(Vessel vessel)
        {
            VesselPartModuleAccess.UpdateVessel(vessel);
        }

        public void VesselUnloaded(Vessel vessel)
        {
            VesselPartModuleAccess.RemoveVessel(vessel.id);
        }
    }
}
