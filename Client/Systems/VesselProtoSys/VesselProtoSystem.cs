using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using System;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.VesselProtoSys
{
    /// <summary>
    /// This system handles the vessel loading into the game and sending our vessel structure to other players.
    /// We only load vesels that are in our subspace
    /// This system also handles changes that a vessel could have. Antenas that extend, shields that open, etc
    /// </summary>
    public class VesselProtoSystem : MessageSystem<VesselProtoSystem, VesselProtoMessageSender, VesselProtoMessageHandler>
    {
        #region Fields & properties

        public ConcurrentDictionary<Guid, VesselProtoUpdate> AllPlayerVessels { get; } =
            new ConcurrentDictionary<Guid, VesselProtoUpdate>();

        public ConcurrentDictionary<Guid, VesselChange> AllPlayerVesselChanges { get; } =
            new ConcurrentDictionary<Guid, VesselChange>();

        public ScreenMessage BannedPartsMessage { get; set; }
        public string BannedPartsStr { get; set; }

        public VesselLoader VesselLoader { get; } = new VesselLoader();

        public bool ProtoSystemReady => Enabled && Time.timeSinceLevelLoad > 1f && FlightGlobals.ready &&
            HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating
            && FlightGlobals.ActiveVessel.state != Vessel.State.DEAD;

        public bool ProtoSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ready && FlightGlobals.ActiveVessel != null ||
            HighLogic.LoadedScene == GameScenes.TRACKSTATION;

        public VesselProtoEvents VesselProtoEvents { get; } = new VesselProtoEvents();

        public VesselRemoveSystem VesselRemoveSystem => SystemsContainer.Get<VesselRemoveSystem>();

        #endregion

        #region Base overrides

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselWasModified.Add(VesselProtoEvents.VesselModified);
            SetupRoutine(new RoutineDefinition(2000, RoutineExecution.Update, CheckVesselsToLoad));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, CheckVesselsToReload));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateBannedPartsMessage));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, ProcessVesselChanges));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.AbandonedVesselsUpdateMsInterval,
                RoutineExecution.Update, SendAbandonedVesselsToServer));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.VesselDefinitionSendMsInterval,
                RoutineExecution.Update, SendVesselDefinition));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselWasModified.Remove(VesselProtoEvents.VesselModified);
            ClearSystem();
            BannedPartsStr = string.Empty;
        }

        #endregion

        #region Public

        /// <summary>
        /// Clears the dictionary, you should call this method when switching scene
        /// </summary>
        public void ClearSystem()
        {
            AllPlayerVessels.Clear();
        }

        /// <summary>
        /// In this method we get the new vessel data and set it to the dictionary of all the player vessels.
        /// </summary>
        public void HandleVesselProtoData(byte[] vesselData, Guid vesselId)
        {
            TaskFactory.StartNew(() =>
            {
                var vesselNode = ConfigNodeSerializer.Deserialize(vesselData);
                if (vesselNode != null && vesselId == Common.ConvertConfigStringToGuid(vesselNode.GetValue("pid")))
                {
                    var newProtoUpd = new VesselProtoUpdate(vesselNode, vesselId);
                    if (newProtoUpd.ProtoVessel == null)
                        return;

                    if (!AllPlayerVessels.TryGetValue(vesselId, out var existingProtoUpd))
                    {
                        AllPlayerVessels.TryAdd(vesselId, newProtoUpd);
                    }
                    else
                    {
                        newProtoUpd.NeedsToBeReloaded = VesselCommon.ProtoVesselNeedsToBeReloaded(existingProtoUpd.Vessel, newProtoUpd.ProtoVessel);
                        TaskFactory.StartNew(() =>
                        {
                            var changes = VesselChanges.GetProtoVesselChanges(existingProtoUpd.ProtoVessel, newProtoUpd.ProtoVessel);
                            if (changes.HasChanges())
                            {
                                AllPlayerVesselChanges.AddOrUpdate(vesselId, changes, (key, existingVal) => changes);
                            }
                        });
                        AllPlayerVessels.TryUpdate(vesselId, newProtoUpd, existingProtoUpd);
                    }
                }
            });
        }

        /// <summary>
        /// Removes a vessel from the loading system. If we receive a protovessel msg after this method is called it will be reloaded
        /// </summary>
        public void RemoveVesselFromLoadingSystem(Guid vesselId)
        {
            AllPlayerVessels.TryRemove(vesselId, out var _);
        }

        /// <summary>
        /// Sets a vessel as unloaded so it can be recreated later. 
        /// For example if you leave a subspace the vessel must still be in the system but it should be unloaded
        /// </summary>
        public void UnloadVesselFromLoadingSystem(Guid vesselId)
        {
            if (AllPlayerVessels.TryGetValue(vesselId, out var existingProtoUpdate))
            {
                AllPlayerVessels.TryUpdate(vesselId, new VesselProtoUpdate(existingProtoUpdate), existingProtoUpdate);
            }
        }

        /// <summary>
        /// Checks the vessel for invalid parts
        /// </summary>
        public bool CheckVessel()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT ||
                FlightGlobals.ActiveVessel == null ||
                !FlightGlobals.ActiveVessel.loaded ||
                VesselCommon.IsSpectating)
                return false;

            if (SystemsContainer.Get<ModSystem>().ModControl != ModControlMode.Disabled)
            {
                BannedPartsStr = GetInvalidVesselParts(FlightGlobals.ActiveVessel);
                return string.IsNullOrEmpty(BannedPartsStr);
            }

            return true;
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Here we run trough all the changes of the dictionary and applies them to the vessel.
        /// We extend/retract antennas, open/close shields, etc...
        /// Once done we just clear the dictionary
        /// </summary>
        private void ProcessVesselChanges()
        {
            try
            {
                if (ProtoSystemBasicReady)
                {
                    foreach (var vesselChange in AllPlayerVesselChanges)
                    {
                        var vessel = FlightGlobals.FindVessel(vesselChange.Key);
                        VesselChanges.ProcessVesselChanges(vessel, vesselChange.Value);
                    }

                    AllPlayerVesselChanges.Clear();
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in ProcessVesselChanges {e}");
            }
        }

        /// <summary>
        /// Send the definition of our own vessel and the secondary vessels. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private void SendVesselDefinition()
        {
            try
            {
                if (ProtoSystemReady && !VesselCommon.ActiveVesselIsInSafetyBubble())
                {
                    MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel);
                    MessageSender.SendVesselMessage(VesselCommon.GetSecondaryVessels());

                    ChangeRoutineExecutionInterval("SendVesselDefinition",
                        VesselCommon.PlayerVesselsNearby()
                            ? SettingsSystem.ServerSettings.VesselDefinitionSendMsInterval
                            : SettingsSystem.ServerSettings.VesselDefinitionSendFarMsInterval);
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in SendVesselDefinition {e}");
            }

        }

        /// <summary>
        /// Prints the banned parts message
        /// </summary>
        private void UpdateBannedPartsMessage()
        {
            try
            {
                if (ProtoSystemReady && !string.IsNullOrEmpty(BannedPartsStr))
                {
                    if (BannedPartsMessage != null)
                        BannedPartsMessage.duration = 0;
                    if (SystemsContainer.Get<ModSystem>().ModControl == ModControlMode.EnabledStopInvalidPartSync)
                        BannedPartsMessage = ScreenMessages.PostScreenMessage($"Active vessel contains the following banned parts, it will not be saved to the server:\n{BannedPartsStr}", 2f, ScreenMessageStyle.UPPER_CENTER);
                    if (SystemsContainer.Get<ModSystem>().ModControl == ModControlMode.EnabledStopInvalidPartLaunch)
                        BannedPartsMessage = ScreenMessages.PostScreenMessage($"Active vessel contains the following banned parts, you will be unable to launch on this server:\n{BannedPartsStr}", 2f, ScreenMessageStyle.UPPER_CENTER);

                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in UpdateBannedPartsMessage {e}");
            }
        }

        /// <summary>
        /// Here we send the vessel that do not have update locks to the server at a given interval. This will update the orbit information etc in the server.
        /// Bear in mind that the server cannot apply "VesselUpdateMessages" over vessel definitions therefore, to update the information of a vessel in the server
        /// we must send all the vessel data.
        /// </summary>
        private void SendAbandonedVesselsToServer()
        {
            try
            {
                if (ProtoSystemBasicReady)
                {
                    MessageSender.SendVesselMessage(VesselCommon.GetAbandonedVessels());
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in SendAbandonedVesselsToServer {e}");
            }
        }

        /// <summary>
        /// Check vessels that must be loaded
        /// </summary>
        private void CheckVesselsToLoad()
        {
            try
            {
                if (ProtoSystemBasicReady && !VesselCommon.ActiveVesselIsInSafetyBubble())
                {
                    //Load vessels that don't exist and are in our subspace
                    var vesselsToLoad = AllPlayerVessels
                        .Where(v => !v.Value.VesselExist && v.Value.ShouldBeLoaded)
                        .ToArray();

                    foreach (var vesselProto in vesselsToLoad)
                    {
                        var vesselWillBeKilled = VesselRemoveSystem.VesselWillBeKilled(vesselProto.Key);
                        if (vesselWillBeKilled)
                            continue;

                        LunaLog.Log($"[LMP]: Loading vessel {vesselProto.Key}");
                        if (VesselLoader.LoadVessel(vesselProto.Value.ProtoVessel))
                        {
                            vesselProto.Value.NeedsToBeReloaded = false;
                            LunaLog.Log($"[LMP]: Vessel {vesselProto.Key} loaded");
                            UpdateVesselProtoInDictionary(vesselProto.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckVesselsToLoad {e}");
            }
        }

        /// <summary>
        /// Check vessels that must be reloaded
        /// </summary>
        private void CheckVesselsToReload()
        {
            try
            {
                if (ProtoSystemBasicReady && !VesselCommon.ActiveVesselIsInSafetyBubble())
                {
                    //Reload vessels that exist
                    var vesselsToReLoad = AllPlayerVessels
                        .Where(pv => pv.Value.NeedsToBeReloaded && pv.Value.VesselExist)
                        .ToArray();

                    foreach (var vesselProto in vesselsToReLoad)
                    {
                        if (VesselRemoveSystem.VesselWillBeKilled(vesselProto.Key))
                            continue;
                        
                        LunaLog.Log($"[LMP]: Reloading vessel {vesselProto.Key}");
                        if (VesselLoader.ReloadVessel(vesselProto.Value.ProtoVessel))
                        {
                            vesselProto.Value.NeedsToBeReloaded = false;
                            LunaLog.Log($"[LMP]: Vessel {vesselProto.Key} reloaded");
                            UpdateVesselProtoInDictionary(vesselProto.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckVesselsToReload {e}");
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Updates the vesselProto from the dictionary in a thread safe manner
        /// </summary>
        public void UpdateVesselProtoInDictionary(VesselProtoUpdate vesselProto)
        {
            AllPlayerVessels.TryGetValue(vesselProto.VesselId, out var existingVesselProto);
            if (existingVesselProto != null)
            {
                AllPlayerVessels.TryUpdate(vesselProto.VesselId, vesselProto, existingVesselProto);
            }
        }

        private static string GetInvalidVesselParts(Vessel checkVessel)
        {
            var bannedParts = checkVessel.BackupVessel().protoPartSnapshots
                .Where(p => !SystemsContainer.Get<ModSystem>().AllowedParts.Contains(p.partName.ToLower())).Distinct().ToArray();

            var bannedPartsStr = bannedParts.Aggregate("", (current, bannedPart) => current + $"{bannedPart}\n");

            LunaLog.Log($"[LMP]: Checked vessel {checkVessel.id } for banned parts, is ok: {bannedParts.Length == 0}");

            return bannedPartsStr;
        }

        // ReSharper disable once UnusedMember.Local
        private void RegisterServerAsteriodIfVesselIsAsteroid(ProtoVessel possibleAsteroid)
        {
            //Register asteroids from other players
            if (possibleAsteroid.vesselType == VesselType.SpaceObject &&
                possibleAsteroid.protoPartSnapshots?.Count == 1 &&
                possibleAsteroid.protoPartSnapshots[0].partName == "PotatoRoid")
                SystemsContainer.Get<AsteroidSystem>().RegisterServerAsteroid(possibleAsteroid.vesselID.ToString());
        }

        #endregion
    }
}
