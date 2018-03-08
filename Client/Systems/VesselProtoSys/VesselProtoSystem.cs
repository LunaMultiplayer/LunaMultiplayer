using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.VesselProtoSys
{
    /// <summary>
    /// This system handles the vessel loading into the game and sending our vessel structure to other players.
    /// We only load vesels that are in our subspace
    /// This system also handles changes that a vessel could have. Antenas that extend, shields that open, etc
    /// </summary>
    public partial class VesselProtoSystem : MessageSystem<VesselProtoSystem, VesselProtoMessageSender, VesselProtoMessageHandler>
    {
        #region Fields & properties

        public static Guid CurrentlyUpdatingVesselId { get; set; } = Guid.Empty;

        public ScreenMessage BannedPartsMessage { get; set; }
        public string BannedPartsStr { get; set; }

        public VesselLoader VesselLoader { get; } = new VesselLoader();

        public bool ProtoSystemReady => Enabled && Time.timeSinceLevelLoad > 10f && FlightGlobals.ready &&
            HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating;

        public bool ProtoSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 10f &&
            HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ready && FlightGlobals.ActiveVessel != null ||
            HighLogic.LoadedScene == GameScenes.TRACKSTATION;

        public VesselProtoEvents VesselProtoEvents { get; } = new VesselProtoEvents();

        public VesselRemoveSystem VesselRemoveSystem => VesselRemoveSystem.Singleton;

        public Queue<Vessel> FlagsToSend = new Queue<Vessel>();

        private static DateTime LastReloadCheck { get; set; } = DateTime.UtcNow;

        private List<Guid> VesselsToRefresh { get; } = new List<Guid>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselProtoSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            TimingManager.FixedUpdateAdd(TimingManager.TimingStage.BetterLateThanNever, CheckVesselsToRefresh);

            GameEvents.onVesselCreate.Add(VesselProtoEvents.VesselCreate);
            GameEvents.onFlightReady.Add(VesselProtoEvents.FlightReady);
            GameEvents.onPartDie.Add(VesselProtoEvents.OnPartDie);
            GameEvents.onGameSceneLoadRequested.Add(VesselProtoEvents.OnSceneRequested);
            GameEvents.onVesselPartCountChanged.Add(VesselProtoEvents.VesselPartCountChanged);

            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, RemoveBadDebrisWhileSpectating));
            SetupRoutine(new RoutineDefinition(2000, RoutineExecution.Update, CheckVesselsToLoad));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateBannedPartsMessage));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, CheckRefreshOwnVesselWhileSpectating));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.VesselPartsSyncMsInterval, RoutineExecution.Update, SendVesselDefinition));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.BetterLateThanNever, CheckVesselsToRefresh);

            GameEvents.onVesselCreate.Remove(VesselProtoEvents.VesselCreate);
            GameEvents.onFlightReady.Remove(VesselProtoEvents.FlightReady);
            GameEvents.onPartDie.Remove(VesselProtoEvents.OnPartDie);
            GameEvents.onGameSceneLoadRequested.Remove(VesselProtoEvents.OnSceneRequested);
            GameEvents.onVesselPartCountChanged.Remove(VesselProtoEvents.VesselPartCountChanged);
            
            //This is the main system that handles the vesselstore so if it's disabled clear the store aswell
            VesselsProtoStore.ClearSystem();
            BannedPartsStr = string.Empty;
        }

        #endregion

        #region Public

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

            if (ModSystem.Singleton.ModControl != ModControlMode.Disabled)
            {
                BannedPartsStr = GetInvalidVesselParts(FlightGlobals.ActiveVessel);
                return string.IsNullOrEmpty(BannedPartsStr);
            }

            return true;
        }

        #endregion

        #region Update routines

        /// <summary>
        /// Check if we have a different part count in the vessel than in the store while spectating and if that's the case we trigger a reload
        /// </summary>
        private void CheckRefreshOwnVesselWhileSpectating()
        {
            if (!VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || !ProtoSystemReady) return;

            if (VesselsProtoStore.AllPlayerVessels.TryGetValue(FlightGlobals.ActiveVessel.id, out var vesselProtoUpdate))
            {
                if (vesselProtoUpdate.ProtoVessel.protoPartSnapshots.Count != FlightGlobals.ActiveVessel.Parts.Count)
                    vesselProtoUpdate.VesselHasUpdate = true;
            }
        }

        /// <summary>
        /// While spectating, the vessel you are watching can crash or something and create debris that shouldn't exist.
        /// Here we run trough all the current vessels that are in your game and remove the ones that are not in the proto store
        /// </summary>
        private static void RemoveBadDebrisWhileSpectating()
        {
            if (VesselCommon.IsSpectating)
            {
                foreach (var vessel in FlightGlobals.Vessels.Where(v => !VesselsProtoStore.AllPlayerVessels.ContainsKey(v.id)))
                {
                    VesselRemoveSystem.Singleton.AddToKillList(vessel.id);
                }
            }
        }


        /// <summary>
        /// Send the definition of our own vessel and the secondary vessels.
        /// </summary>
        private void SendVesselDefinition()
        {
            try
            {
                if (ProtoSystemReady)
                {
                    MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, false);
                    MessageSender.SendVesselMessage(VesselCommon.GetSecondaryVessels());
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
                    if (ModSystem.Singleton.ModControl == ModControlMode.EnabledStopInvalidPartSync)
                        BannedPartsMessage = ScreenMessages.PostScreenMessage($"Active vessel contains the following banned parts, it will not be saved to the server:\n{BannedPartsStr}", 2f, ScreenMessageStyle.UPPER_CENTER);
                    if (ModSystem.Singleton.ModControl == ModControlMode.EnabledStopInvalidPartLaunch)
                        BannedPartsMessage = ScreenMessages.PostScreenMessage($"Active vessel contains the following banned parts, you will be unable to launch on this server:\n{BannedPartsStr}", 2f, ScreenMessageStyle.UPPER_CENTER);

                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in UpdateBannedPartsMessage {e}");
            }
        }

        /// <summary>
        /// Check vessels that must be loaded
        /// </summary>
        private void CheckVesselsToLoad()
        {
            try
            {
                if (ProtoSystemBasicReady)
                {
                    //Load vessels that don't exist, are in our subspace and out of safety bubble
                    var vesselsToLoad = VesselsProtoStore.AllPlayerVessels.Where(v => !v.Value.VesselExist && v.Value.ShouldBeLoaded);

                    foreach (var vesselProto in vesselsToLoad)
                    {
                        if (VesselRemoveSystem.VesselWillBeKilled(vesselProto.Key))
                            continue;

                        //Only load vessels that are in safety bubble on the track station
                        if (vesselProto.Value.IsInSafetyBubble && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                            continue;

                        if (VesselCommon.ActiveVesselIsInSafetyBubble() && VesselCommon.IsNearKsc(vesselProto.Value.ProtoVessel, 20000))
                            continue;

                        LunaLog.Log($"[LMP]: Loading vessel {vesselProto.Key}");

                        CurrentlyUpdatingVesselId = vesselProto.Key;
                        if (VesselLoader.LoadVessel(vesselProto.Value.ProtoVessel))
                        {
                            LunaLog.Log($"[LMP]: Vessel {vesselProto.Key} loaded");
                        }
                        CurrentlyUpdatingVesselId = Guid.Empty;
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
        private void CheckVesselsToRefresh()
        {
            try
            {
                if ((DateTime.UtcNow - LastReloadCheck).TotalMilliseconds > 1500 && ProtoSystemBasicReady && !VesselCommon.ActiveVesselIsInSafetyBubble())
                {
                    VesselsToRefresh.Clear();

                    //We get the vessels that already exist
                    VesselsToRefresh.AddRange(VesselsProtoStore.AllPlayerVessels
                        .Where(pv => pv.Value.VesselExist && pv.Value.VesselHasUpdate)
                        .Select(v => v.Key));

                    //Do not iterate directly trough the AllPlayerVessels dictionary as the collection can be modified in another threads!
                    foreach (var vesselIdToReload in VesselsToRefresh)
                    {
                        if (VesselRemoveSystem.VesselWillBeKilled(vesselIdToReload))
                            continue;

                        //Do not handle vessel proto updates over our OWN active vessel if we are not spectating
                        //If there is an undetected dock (our protovessel has been modified) it will be detected
                        //in the docksystem
                        if (vesselIdToReload == FlightGlobals.ActiveVessel?.id && !VesselCommon.IsSpectating)
                            continue;

                        if (VesselsProtoStore.AllPlayerVessels.TryGetValue(vesselIdToReload, out var vesselProtoUpdate))
                        {
                            CurrentlyUpdatingVesselId = vesselIdToReload;
                            ProtoToVesselRefresh.UpdateVesselPartsFromProtoVessel(vesselProtoUpdate.Vessel, vesselProtoUpdate.ProtoVessel, vesselProtoUpdate.VesselParts.Keys);
                            vesselProtoUpdate.VesselHasUpdate = false;
                            CurrentlyUpdatingVesselId = Guid.Empty;
                        }
                    }

                    LastReloadCheck = DateTime.UtcNow;
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckVesselsToReload {e}");
            }
        }

        #endregion

        #region Private

        private static string GetInvalidVesselParts(Vessel checkVessel)
        {
            var bannedParts = checkVessel.BackupVessel().protoPartSnapshots
                .Where(p => !ModSystem.Singleton.AllowedParts.Contains(p.partName.ToLower())).Distinct();

            var bannedPartsStr = bannedParts.Aggregate("", (current, bannedPart) => current + $"{bannedPart}\n");

            return bannedPartsStr;
        }

        // ReSharper disable once UnusedMember.Local
        private void RegisterServerAsteriodIfVesselIsAsteroid(ProtoVessel possibleAsteroid)
        {
            //Register asteroids from other players
            if (possibleAsteroid.vesselType == VesselType.SpaceObject &&
                possibleAsteroid.protoPartSnapshots?.Count == 1 &&
                possibleAsteroid.protoPartSnapshots[0].partName == "PotatoRoid")
                AsteroidSystem.Singleton.RegisterServerAsteroid(possibleAsteroid.vesselID.ToString());
        }

        #endregion
    }
}
