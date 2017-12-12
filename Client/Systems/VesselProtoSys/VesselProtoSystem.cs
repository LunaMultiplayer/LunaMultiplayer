using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
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

        public Queue<Vessel> FlagsToSend = new Queue<Vessel>();

        private static DateTime LastReloadCheck { get; set; } = DateTime.UtcNow;

        #endregion

        #region Base overrides

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            TimingManager.FixedUpdateAdd(TimingManager.TimingStage.BetterLateThanNever, CheckVesselsToReload);

            GameEvents.onVesselWasModified.Add(VesselProtoEvents.VesselModified);
            GameEvents.onVesselGoOnRails.Add(VesselProtoEvents.VesselGoOnRails);
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, CheckVesselsToLoadReloadWhileNotInFlight));
            SetupRoutine(new RoutineDefinition(2000, RoutineExecution.Update, CheckVesselsToLoad));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateBannedPartsMessage));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.VesselDefinitionSendMsInterval,
                RoutineExecution.Update, SendVesselDefinition));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.BetterLateThanNever, CheckVesselsToReload);

            GameEvents.onVesselWasModified.Remove(VesselProtoEvents.VesselModified);
            GameEvents.onVesselGoOnRails.Remove(VesselProtoEvents.VesselGoOnRails);

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

            if (SystemsContainer.Get<ModSystem>().ModControl != ModControlMode.Disabled)
            {
                BannedPartsStr = GetInvalidVesselParts(FlightGlobals.ActiveVessel);
                return string.IsNullOrEmpty(BannedPartsStr);
            }

            return true;
        }

        #endregion

        #region Update routines

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
                    MessageSender.SendVesselMessage(VesselCommon.GetSecondaryVessels(), false);
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
        /// Check vessels that must be loaded or reloaded while we are in a different scene than in flight
        /// </summary>
        private void CheckVesselsToLoadReloadWhileNotInFlight()
        {
            if (Enabled && Time.timeSinceLevelLoad > 1f && HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                HighLogic.CurrentGame.flightState.protoVessels.Clear();
                var protoVessels = VesselsProtoStore.AllPlayerVessels.Values.Where(v => v.ShouldBeLoaded).Select(v => v.ProtoVessel);
                HighLogic.CurrentGame.flightState.protoVessels.AddRange(protoVessels);
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
                    var vesselsToLoad = VesselsProtoStore.AllPlayerVessels
                        .Where(v => !v.Value.VesselExist && v.Value.ShouldBeLoaded);

                    foreach (var vesselProto in vesselsToLoad)
                    {
                        if (VesselRemoveSystem.VesselWillBeKilled(vesselProto.Key))
                            continue;

                        LunaLog.Log($"[LMP]: Loading vessel {vesselProto.Key}");
                        if (VesselLoader.LoadVessel(vesselProto.Value.ProtoVessel))
                        {
                            LunaLog.Log($"[LMP]: Vessel {vesselProto.Key} loaded");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckVesselsToLoad {e}");
            }
        }

        private List<Guid> VesselsToReload { get; } = new List<Guid>();
        
        /// <summary>
        /// Check vessels that must be reloaded
        /// </summary>
        private void CheckVesselsToReload()
        {
            try
            {
                if ((DateTime.UtcNow - LastReloadCheck).TotalMilliseconds > 1500 && ProtoSystemBasicReady && !VesselCommon.ActiveVesselIsInSafetyBubble())
                {
                    VesselsToReload.Clear();
                    //We get the vessels that already exist. 
                    VesselsToReload.AddRange(VesselsProtoStore.AllPlayerVessels.Where(pv => pv.Value.VesselExist && !pv.Value.UpdatesChecked).Select(v => v.Key));

                    //Do not iterate directly trough the AllPlayerVessels dictionary as the collection can be modified in another threads!
                    foreach (var vesselIdToReload in VesselsToReload)
                    {
                        if (VesselRemoveSystem.VesselWillBeKilled(vesselIdToReload))
                            continue;

                        //Do not handle vessel proto updates over our OWN active vessel if we are not spectating
                        if (vesselIdToReload == FlightGlobals.ActiveVessel?.id && !VesselCommon.IsSpectating)
                            continue;

                        if (VesselsProtoStore.AllPlayerVessels.TryGetValue(vesselIdToReload, out var vesselProtoUpdate))
                        {
                            VesselUpdater.UpdateVesselPartsFromProtoVessel(vesselProtoUpdate.Vessel, vesselProtoUpdate.ProtoVessel);
                            vesselProtoUpdate.UpdatesChecked = true;
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
                .Where(p => !SystemsContainer.Get<ModSystem>().AllowedParts.Contains(p.partName.ToLower())).Distinct();

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
                SystemsContainer.Get<AsteroidSystem>().RegisterServerAsteroid(possibleAsteroid.vesselID.ToString());
        }

        #endregion
    }
}
