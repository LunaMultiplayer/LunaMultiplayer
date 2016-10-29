using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselWarpSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
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
        public List<VesselProtoUpdate> AllPlayerVessels { get; } = new List<VesselProtoUpdate>();

        public float CheckVesselsToLoadSInterval = 2.5f;
        public float UpdateScreenMessageInterval = 1f;
        public float SendVesselToServerSInterval = 5f;

        public ScreenMessage BannedPartsMessage { get; set; }
        public string BannedPartsStr { get; set; }

        public VesselLoader VesselLoader { get; } = new VesselLoader();
        private VesselProtoEvents VesselProtoEvents { get; } = new VesselProtoEvents();

        public bool CurrentVesselSent { get; set; }
        public bool VesselReady { get; set; } = false;

        private static bool VesselProtoSystemReady
            => HighLogic.LoadedScene == GameScenes.FLIGHT && Time.timeSinceLevelLoad > 1f && FlightGlobals.ready;

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(SendVesselsToServer());
            Client.Singleton.StartCoroutine(SendAbandonedVesselsToServer());
            Client.Singleton.StartCoroutine(CheckVesselsToLoad());
            Client.Singleton.StartCoroutine(UpdateBannedPartsMessage());

            GameEvents.onFlightReady.Add(VesselProtoEvents.OnFlightReady);
            GameEvents.onVesselWasModified.Add(VesselProtoEvents.OnVesselWasModified);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onFlightReady.Remove(VesselProtoEvents.OnFlightReady);
            GameEvents.onVesselWasModified.Remove(VesselProtoEvents.OnVesselWasModified);
            AllPlayerVessels.Clear();
            BannedPartsStr = string.Empty;
            CurrentVesselSent = false;
            VesselReady = false;
        }

        /// <summary>
        /// Here we send our vessel and load other people vessels if they are in our subspace and have updates for them.
        /// </summary>
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!VesselProtoSystemReady)
                return;

            //Never send the proto vessel if we are in safety bubble!
            if (!CurrentVesselSent && VesselReady && !VesselCommon.ActiveVesselIsInSafetyBubble())
            {
                CurrentVesselSent = true;
                MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel);
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

                    if (VesselProtoSystemReady)
                    {
                        foreach (var vessel in FlightGlobals.VesselsUnloaded.Where(v => !LockSystem.Singleton.LockExists("update-" + v.id)))
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

        /// <summary>
        /// Here we send our vessel to the server at a given interval. This will update our active vessel position orbit information etc in the server.
        /// Bear in mind that the server cannot apply "VesselUpdateMessages" over vessel definitions therefore, to update the information of a vessel in the server
        /// we must send all the vessel data.
        /// </summary>
        private IEnumerator SendVesselsToServer()
        {
            var seconds = new WaitForSeconds((float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselDefinitionUpdateMsInterval).TotalSeconds);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

                    if (VesselProtoSystemReady)
                    {
                        var updateVesselIds = LockSystem.Singleton.GetLocksPrefix(SettingsSystem.CurrentSettings.PlayerName, "update-").Select(l => l.Substring(7));

                        foreach (var id in updateVesselIds)
                        {
                            MessageSender.SendVesselMessage(FlightGlobals.FindVessel(new Guid(id)));
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Coroutine error in SendVesselToServer {e}");
                }

                yield return seconds;
            }
        }

        /// <summary>
        /// Check vessels that must be loaded
        /// </summary>
        private IEnumerator CheckVesselsToLoad()
        {
            var seconds = new WaitForSeconds(CheckVesselsToLoadSInterval);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

                    if (VesselProtoSystemReady)
                    {
                        //Load vessels when we have at least 1 update for them and are in our subspace
                        var vesselsToLoad = AllPlayerVessels
                            .Where(v => v.HasUpdates && !v.Loaded
                            && VesselWarpSystem.Singleton.GetVesselSubspace(v.VesselId) == WarpSystem.Singleton.CurrentSubspace)
                            .ToArray();

                        foreach (var vesselProto in vesselsToLoad)
                        {
                            Client.Singleton.StartCoroutine(VesselLoader.LoadVessel(vesselProto.VesselNode, vesselProto.VesselId));
                            vesselProto.Loaded = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Coroutine error in CheckVesselsToLoad {e}");
                }

                yield return seconds;
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
                VesselLockSystem.Singleton.IsSpectating)
                return false;

            if (ModSystem.Singleton.ModControl != ModControlMode.DISABLED)
            {
                BannedPartsStr = GetInvalidVesselParts(FlightGlobals.ActiveVessel);
                return string.IsNullOrEmpty(BannedPartsStr);
            }

            return true;
        }

        private IEnumerator UpdateBannedPartsMessage()
        {
            var seconds = new WaitForSeconds(UpdateScreenMessageInterval);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

                    if (VesselProtoSystemReady && !string.IsNullOrEmpty(BannedPartsStr))
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
    }
}
