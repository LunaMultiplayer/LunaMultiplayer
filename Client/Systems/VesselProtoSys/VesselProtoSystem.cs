using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Systems.VesselProtoSys
{
    /// <summary>
    /// This system handles the vessel loading into the game and sending our vessel structure to other players.
    /// We only load vesels that are in our subspace
    /// </summary>
    public class VesselProtoSystem :
        MessageSystem<VesselProtoSystem, VesselProtoMessageSender, VesselProtoMessageHandler>
    {

        #region Fields
        
        public ConcurrentDictionary<Guid,VesselProtoUpdate> AllPlayerVessels { get; } = 
            new ConcurrentDictionary<Guid, VesselProtoUpdate>();

        private static float VesselDefinitionSendSInterval =>
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselDefinitionSendMsInterval).TotalSeconds;

        private static float VesselDefinitionSendFarSInterval =>
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselDefinitionSendFarMsInterval).TotalSeconds;

        public int CheckVesselsToLoadMsInterval = 2500;
        public float UpdateScreenMessageInterval = 1f;

        public ScreenMessage BannedPartsMessage { get; set; }
        public string BannedPartsStr { get; set; }

        public VesselLoader VesselLoader { get; } = new VesselLoader();

        public bool ProtoSystemReady => Enabled && Time.timeSinceLevelLoad > 1f && FlightGlobals.ready &&
            HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating
            && FlightGlobals.ActiveVessel.state != Vessel.State.DEAD;

        public bool ProtoSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ready && FlightGlobals.ActiveVessel != null) ||
            (HighLogic.LoadedScene == GameScenes.TRACKSTATION);

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(SendVesselDefinition());
            Client.Singleton.StartCoroutine(SendAbandonedVesselsToServer());
            Client.Singleton.StartCoroutine(UpdateBannedPartsMessage());
        }

        /// <summary>
        /// Check vessels that must be loaded
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (Enabled && ProtoSystemBasicReady && Timer.ElapsedMilliseconds > CheckVesselsToLoadMsInterval)
            {
                //Reload vessels that exist
                var vesselsToReLoad = AllPlayerVessels
                   .Where(v => !v.Value.Loaded && FlightGlobals.Vessels.Any(vl => vl.id == v.Key))
                   .ToArray();

                foreach (var vesselProto in vesselsToReLoad)
                {
                    VesselLoader.ReloadVessel(vesselProto.Value);
                }

                //Load vessels that don't exist and are in our subspace
                var vesselsToLoad = AllPlayerVessels
                    .Where(v => !v.Value.Loaded && FlightGlobals.Vessels.All(vl => vl.id != v.Key) &&
                    (SettingsSystem.ServerSettings.ShowVesselsInThePast || !VesselCommon.VesselIsControlledAndInPastSubspace(v.Key)))
                    .ToArray();

                foreach (var vesselProto in vesselsToLoad)
                {
                    VesselLoader.LoadVessel(vesselProto.Value);
                }

                ResetTimer();
            }
        }

        public override void OnDisabled()
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
                VesselProtoUpdate val;
                AllPlayerVessels.TryRemove(vesselId, out val);
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

            if (ModSystem.Singleton.ModControl != ModControlMode.DISABLED)
            {
                BannedPartsStr = GetInvalidVesselParts(FlightGlobals.ActiveVessel);
                return string.IsNullOrEmpty(BannedPartsStr);
            }

            return true;
        }

        #endregion

        #region Private

        #region Coroutines
        
        /// <summary>
        /// Send the definition of our own vessel and the secondary vessels. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private IEnumerator SendVesselDefinition()
        {
            var seconds = new WaitForSeconds(VesselDefinitionSendSInterval);
            var secondsFar = new WaitForSeconds(VesselDefinitionSendFarSInterval);

            while (true)
            {
                try
                {
                    if (!Enabled)
                        break;

                    if (ProtoSystemReady)
                    {
                        if (!VesselCommon.ActiveVesselIsInSafetyBubble())
                            MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel);

                        foreach (var vessel in VesselCommon.GetSecondaryVessels())
                        {
                            MessageSender.SendVesselMessage(vessel);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Coroutine error in SendVesselDefinition {e}");
                }

                if (VesselCommon.PlayerVesselsNearby())
                    yield return seconds;
                else
                    yield return secondsFar;
            }
        }

        private IEnumerator UpdateBannedPartsMessage()
        {
            var seconds = new WaitForSeconds(UpdateScreenMessageInterval);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

                    if (!string.IsNullOrEmpty(BannedPartsStr) && ProtoSystemReady)
                    {
                        if (BannedPartsMessage != null)
                            BannedPartsMessage.duration = 0;
                        if (ModSystem.Singleton.ModControl == ModControlMode.ENABLED_STOP_INVALID_PART_SYNC)
                            BannedPartsMessage = ScreenMessages.PostScreenMessage(
                                    "Active vessel contains the following banned parts, it will not be saved to the server:\n" + BannedPartsStr, 2f, ScreenMessageStyle.UPPER_CENTER);
                        if (ModSystem.Singleton.ModControl == ModControlMode.ENABLED_STOP_INVALID_PART_LAUNCH)
                            BannedPartsMessage = ScreenMessages.PostScreenMessage(
                                    "Active vessel contains the following banned parts, you will be unable to launch on this server:\n" +
                                    BannedPartsStr, 2f, ScreenMessageStyle.UPPER_CENTER);

                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Coroutine error in UpdateBannedPartsMessage {e}");
                }

                yield return seconds;
            }
        }

        /// <summary>
        /// Here we send the vessel that do not have update locks to the server at a given interval. This will update the orbit information etc in the server.
        /// Bear in mind that the server cannot apply "VesselUpdateMessages" over vessel definitions therefore, to update the information of a vessel in the server
        /// we must send all the vessel data.
        /// </summary>
        private IEnumerator SendAbandonedVesselsToServer()
        {
            var seconds = new WaitForSeconds((float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.AbandonedVesselsUpdateMsInterval).TotalSeconds);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

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
                    Debug.LogError($"[LMP]: Coroutine error in SendAbandonedVesselsToServer {e}");
                }

                yield return seconds;
            }
        }

        #endregion

        private static string GetInvalidVesselParts(Vessel checkVessel)
        {
            var bannedParts = checkVessel.BackupVessel().protoPartSnapshots
                .Where(p => !ModSystem.Singleton.AllowedParts.Contains(p.partName.ToLower())).Distinct().ToArray();

            var bannedPartsStr = bannedParts.Aggregate("", (current, bannedPart) => current + (bannedPart + "\n"));

            Debug.Log($"[LMP]: Checked vessel {checkVessel.id } for banned parts, is ok: {bannedParts.Length == 0}");

            return bannedPartsStr;
        }

        private void RegisterServerAsteriodIfVesselIsAsteroid(ProtoVessel possibleAsteroid)
        {
            //Register asteroids from other players
            if ((possibleAsteroid.vesselType == VesselType.SpaceObject) &&
                (possibleAsteroid.protoPartSnapshots?.Count == 1) &&
                (possibleAsteroid.protoPartSnapshots[0].partName == "PotatoRoid"))
                AsteroidSystem.Singleton.RegisterServerAsteroid(possibleAsteroid.vesselID.ToString());
        }

        #endregion
    }
}
