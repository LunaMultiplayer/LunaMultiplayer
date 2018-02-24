using LunaClient.Base;
using LunaClient.ModuleStore;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselPartModuleSyncSystem : MessageSystem<VesselPartModuleSyncSystem, VesselPartModuleSyncMessageSender, VesselPartModuleSyncMessageHandler>
    {
        #region Fields & properties

        public bool PartSyncSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT && Time.timeSinceLevelLoad > 3f;

        private List<Vessel> SecondaryVesselsToUpdate { get; } = new List<Vessel>();

        public static readonly Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>> LastUpdatedDictionary =
            new Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, PartSyncUpdateEntry>>>>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselPartModuleSyncSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(10, RoutineExecution.Update, SendVesselPartUpdates));
            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, SendSecondaryVesselPartUpdates));
        }

        #endregion

        #region Update routines

        private void SendVesselPartUpdates()
        {
            if (PartSyncSystemReady && !VesselCommon.IsSpectating)
            {
                CheckAndSendVesselUpdate(FlightGlobals.ActiveVessel);
            }
        }

        private void SendSecondaryVesselPartUpdates()
        {
            if (PartSyncSystemReady && !VesselCommon.IsSpectating)
            {
                SecondaryVesselsToUpdate.Clear();
                SecondaryVesselsToUpdate.AddRange(VesselCommon.GetSecondaryVessels());

                for (var i = 0; i < SecondaryVesselsToUpdate.Count; i++)
                {
                    CheckAndSendVesselUpdate(SecondaryVesselsToUpdate[i]);
                }
            }
        }

        #endregion

        #region Private methods

        public void CheckAndSendVesselUpdate(Vessel vessel)
        {
            if (vessel == null || !vessel.loaded || vessel.protoVessel == null) return;

            for (var i = 0; i < vessel.Parts.Count; i++)
            {
                var part = vessel.parts[i];
                for (var j = 0; j < part.Modules.Count; j++)
                {
                    var module = part.Modules[j];
                    if (FieldModuleStore.InheritanceTypeChain.TryGetValue(module.moduleName, out var inheritChain))
                    {
                        foreach (var moduleName in inheritChain)
                        {
                            if (FieldModuleStore.ModuleFieldsDictionary.TryGetValue(moduleName, out var definition))
                            {
                                foreach (var fieldInfo in definition.PersistentModuleField)
                                {
                                    var customizationResult = SkipSending(vessel.id, part.flightID, moduleName, fieldInfo.Name);

                                    //TODO reflection get generates 8kb... Maybe we can use fastmembers?
                                    var fieldVal = fieldInfo.GetValue(module).ToInvariantString();
                                    var snapshotVal = module.snapshot?.moduleValues.GetValue(fieldInfo.Name);

                                    if (snapshotVal != null && fieldVal != null && fieldVal != snapshotVal)
                                    {
                                        switch (customizationResult)
                                        {
                                            case CustomizationResult.TooEarly: //Do not update anything and wait until next time
                                                break;
                                            case CustomizationResult.IgnoreSend: //Update our proto only
                                                module.snapshot?.moduleValues?.SetValue(fieldInfo.Name, fieldVal);
                                                break;
                                            case CustomizationResult.Ok:
                                                module.snapshot?.moduleValues?.SetValue(fieldInfo.Name, fieldVal);
                                                LunaLog.Log($"Detected a part module change. FlightId: {part.flightID} PartName: {part.name} Module: {moduleName} Field: {fieldInfo.Name}");
                                                MessageSender.SendVesselPartSyncMsg(vessel.id, part.flightID, module.moduleName, fieldInfo.Name, fieldVal);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static CustomizationResult SkipSending(Guid vesselId, uint partFlightId, string moduleName, string fieldName)
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

        #endregion
    }
}
