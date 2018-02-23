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

        public static readonly Dictionary<Guid, PartSyncUpdateEntry> LastUpdatedDictionary = new Dictionary<Guid, PartSyncUpdateEntry>();

        private static readonly List<Type> InheritTypeChain = new List<Type>();

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
                                    var fieldCustomization = FieldModuleStore.GetCustomFieldDefinition(moduleName, fieldInfo.Name);
                                    if (fieldCustomization.IgnoreSend) continue;
                                    if (LastUpdatedDictionary.TryGetValue(vessel.id, out var lastUpdateValue) && !lastUpdateValue.TimeToCheck) continue;

                                    if (!LastUpdatedDictionary.ContainsKey(vessel.id))
                                        LastUpdatedDictionary.Add(vessel.id, new PartSyncUpdateEntry(fieldCustomization.IntervalCheckChangesMs));
                                    else
                                        LastUpdatedDictionary[vessel.id].Update();

                                    var fieldVal = fieldInfo.GetValue(module).ToInvariantString();
                                    var snapshotVal = module.snapshot?.moduleValues.GetValue(fieldInfo.Name);

                                    if (snapshotVal != null && fieldVal != null && fieldVal != snapshotVal)
                                    {
                                        vessel.protoVessel.protoPartSnapshots[i].modules[j].moduleValues.SetValue(fieldInfo.Name, fieldVal);
                                        LunaLog.Log($"Detected a part module change. FlightId: {part.flightID} PartName: {part.partName} Module: {moduleName} Field: {fieldInfo.Name}");
                                        MessageSender.SendVesselPartSyncMsg(vessel.id, part.flightID, module.moduleName, fieldInfo.Name, fieldVal);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        #endregion
    }
}
