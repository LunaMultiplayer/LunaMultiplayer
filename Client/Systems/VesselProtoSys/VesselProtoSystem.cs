using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselUtilities;
using LunaCommon.Enums;
using System;
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

        #endregion

        #region Base overrides

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselWasModified.Add(VesselProtoEvents.VesselModified);
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, CheckVesselsToLoadReloadWhileNotInFlight));
            SetupRoutine(new RoutineDefinition(2000, RoutineExecution.Update, CheckVesselsToLoad));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, CheckVesselsToReload));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateBannedPartsMessage));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.VesselDefinitionSendMsInterval,
                RoutineExecution.Update, SendVesselDefinition));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselWasModified.Remove(VesselProtoEvents.VesselModified);

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
        /// Send the definition of our own vessel and the secondary vessels. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private void SendVesselDefinition()
        {
            try
            {
                if (ProtoSystemReady)
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

        /// <summary>
        /// Check vessels that must be reloaded
        /// </summary>
        private void CheckVesselsToReload()
        {
            try
            {
                if (ProtoSystemBasicReady && !VesselCommon.ActiveVesselIsInSafetyBubble())
                {
                    //We run trough the vessels that already exist
                    var vesselsToReLoad = VesselsProtoStore.AllPlayerVessels.Where(pv => pv.Value.VesselExist);

                    foreach (var vesselProto in vesselsToReLoad)
                    {
                        if (VesselRemoveSystem.VesselWillBeKilled(vesselProto.Key))
                            continue;

                        if (!VesselCommon.ProtoVesselNeedsToBeReloaded(vesselProto.Value.Vessel, vesselProto.Value.ProtoVessel))
                            continue;

                        LunaLog.Log($"[LMP]: Reloading vessel {vesselProto.Key}");
                        if (VesselLoader.ReloadVessel(vesselProto.Value.ProtoVessel))
                        {
                            LunaLog.Log($"[LMP]: Vessel {vesselProto.Key} reloaded");
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
