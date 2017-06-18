using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselPositionSys;
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
    /// </summary>
    public class VesselProtoSystem : MessageSystem<VesselProtoSystem, VesselProtoMessageSender, VesselProtoMessageHandler>
    {
        #region Fields & properties

        public ConcurrentDictionary<Guid, VesselProtoUpdate> AllPlayerVessels { get; } =
            new ConcurrentDictionary<Guid, VesselProtoUpdate>();

        public ScreenMessage BannedPartsMessage { get; set; }
        public string BannedPartsStr { get; set; }

        public VesselLoader VesselLoader { get; } = new VesselLoader();

        public bool ProtoSystemReady => Enabled && Time.timeSinceLevelLoad > 1f && FlightGlobals.ready &&
            HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating
            && FlightGlobals.ActiveVessel.state != Vessel.State.DEAD;

        public bool ProtoSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f && HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ready && FlightGlobals.ActiveVessel != null ||
            HighLogic.LoadedScene == GameScenes.TRACKSTATION;

        #endregion

        #region Base overrides

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, CheckVesselsToLoad));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateBannedPartsMessage));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.AbandonedVesselsUpdateMsInterval,
                RoutineExecution.Update, SendAbandonedVesselsToServer));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.VesselDefinitionSendMsInterval,
                RoutineExecution.Update, SendVesselDefinition));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            AllPlayerVessels.Clear();
            BannedPartsStr = string.Empty;
        }

        #endregion

        #region Public

        /// <summary>
        /// Removes a vessel from the loading system. If we receive a protovessel msg after this method is called it will be reloaded
        /// </summary>
        /// <param name="vesselId"></param>
        public void RemoveVesselFromLoadingSystem(Guid vesselId)
        {
            if (AllPlayerVessels.ContainsKey(vesselId))
            {
                AllPlayerVessels.TryRemove(vesselId, out var _);
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
        /// Send the definition of our own vessel and the secondary vessels. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private void SendVesselDefinition()
        {
            try
            {
                if (ProtoSystemReady)
                {
                    if (!VesselCommon.ActiveVesselIsInSafetyBubble())
                        MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel);

                    foreach (var vessel in VesselCommon.GetSecondaryVessels())
                    {
                        MessageSender.SendVesselMessage(vessel);
                    }

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
                    foreach (var vessel in VesselCommon.GetAbandonedVessels())
                    {
                        MessageSender.SendVesselMessage(vessel);
                    }
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
                if (ProtoSystemBasicReady)
                {
                    //Try to remove proto messages to own vessel
                    if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel != null)
                        AllPlayerVessels.TryRemove(FlightGlobals.ActiveVessel.id, out _);

                    //Reload vessels that exist
                    var vesselsToReLoad = AllPlayerVessels
                       .Where(v => !v.Value.Loaded && FlightGlobals.Vessels.Any(vl => vl.id == v.Value.VesselId))
                       .ToArray();

                    foreach (var vesselProto in vesselsToReLoad)
                    {
                        bool vesselLoaded = VesselLoader.ReloadVesselIfChanged(vesselProto.Value);
                        if (vesselLoaded && !SettingsSystem.CurrentSettings.UseAlternativePositionSystem)
                        {
                            //TODO: This call will not put the vessel in the right position if a long time has elapsed between when the update was generated and when it's being applied, if in atmo.
                            //TODO: This is because positions are set via ballistic orbits, which don't extrapolate properly in atmo.
                            SystemsContainer.Get<VesselPositionSystem>().UpdateVesselPositionOnNextFixedUpdate(vesselProto.Value.VesselId);
                        }
                    }

                    //Load vessels that don't exist and are in our subspace
                    var vesselsToLoad = AllPlayerVessels
                        .Where(v => !v.Value.Loaded && FlightGlobals.Vessels.All(vl => vl.id != v.Value.VesselId) &&
                        (SettingsSystem.ServerSettings.ShowVesselsInThePast || !VesselCommon.VesselIsControlledAndInPastSubspace(v.Value.VesselId)))
                        .ToArray();

                    foreach (var vesselProto in vesselsToLoad)
                    {
                        VesselLoader.LoadVessel(vesselProto.Value);

                        if (!SettingsSystem.CurrentSettings.UseAlternativePositionSystem)
                        {
                            //TODO: This call will not put the vessel in the right position if a long time has elapsed between when the update was generated and when it's being applied, if in atmo.
                            //TODO: This is because positions are set via ballistic orbits, which don't extrapolate properly in atmo.
                            SystemsContainer.Get<VesselPositionSystem>().UpdateVesselPositionOnNextFixedUpdate(vesselProto.Value.VesselId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckVesselsToLoad {e}");
            }
        }

        #endregion

        #region Private

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
