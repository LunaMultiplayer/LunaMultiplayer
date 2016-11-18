using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Lock;
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

        private static VesselRanges LmpRanges { get; } = new VesselRanges
        {
            escaping = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.escaping)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            flying = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.flying)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            landed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.landed)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            orbit = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            prelaunch = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            splashed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                unpack = 0.01f,
                pack = 0.1f
            },
            subOrbital = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit)
            {
                unpack = 0.01f,
                pack = 0.1f
            }
        };

        public List<VesselProtoUpdate> AllPlayerVessels { get; } = new List<VesselProtoUpdate>();

        private static float VesselDefinitionSendSInterval =>
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselDefinitionSendMsInterval).TotalSeconds;

        private static float VesselDefinitionSendFarSInterval =>
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselDefinitionSendFarMsInterval).TotalSeconds;

        public float CheckVesselsToLoadSInterval = 2.5f;
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

        private const float VesselPackCheckSInterval = 1f;

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(SendVesselDefinition());
            Client.Singleton.StartCoroutine(SendAbandonedVesselsToServer());
            Client.Singleton.StartCoroutine(CheckVesselsToLoad());
            Client.Singleton.StartCoroutine(UpdateBannedPartsMessage());
            Client.Singleton.StartCoroutine(PackControlledVessels());
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            AllPlayerVessels.Clear();
            BannedPartsStr = string.Empty;
        }

        /// <summary>
        /// Set all other vessels as packed so the movement is better
        /// </summary>
        private IEnumerator PackControlledVessels()
        {
            var seconds = new WaitForSeconds(VesselPackCheckSInterval);
            while (true)
            {
                try
                {
                    if (!Enabled)
                        break;

                    if (ProtoSystemReady)
                    {
                        var controlledVessels = VesselCommon.GetControlledVessels();
                        
                        foreach (var vessel in FlightGlobals.Vessels.Where(v=> v.id != FlightGlobals.ActiveVessel.id))
                        {
                            if (controlledVessels.Contains(vessel))
                            {
                                if (!vessel.packed) PackVessel(vessel);
                            }
                            else
                            {
                                UnPackVessel(vessel);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Coroutine error in PackControlledVessels {e}");
                }

                yield return seconds;
            }
        }

        private static void UnPackVessel(Vessel vessel)
        {
            vessel.vesselRanges = PhysicsGlobals.Instance.VesselRangesDefault;
        }

        private static void PackVessel(Vessel vessel)
        {
            vessel.vesselRanges = LmpRanges;
        }

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

        #endregion

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

                    if (ProtoSystemBasicReady)
                    {
                        //Reload vessels that exist
                        var vesselsToReLoad = AllPlayerVessels
                           .Where(v => !v.Loaded && FlightGlobals.Vessels.Any(vl => vl.id == v.VesselId))
                           .ToArray();

                        foreach (var vesselProto in vesselsToReLoad)
                        {
                            VesselLoader.ReloadVessel(vesselProto);
                        }

                        //Load vessels that don't exist and are in our subspace
                        var vesselsToLoad = AllPlayerVessels
                            .Where(v => !v.Loaded && FlightGlobals.Vessels.All(vl => vl.id != v.VesselId) &&
                            (SettingsSystem.ServerSettings.ShowVesselsInThePast || !VesselCommon.VesselIsControlledAndInPastSubspace(v.VesselId)))
                            .ToArray();

                        foreach (var vesselProto in vesselsToLoad)
                        {
                            VesselLoader.LoadVessel(vesselProto);
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
                VesselCommon.IsSpectating)
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

                    if (ProtoSystemReady && !string.IsNullOrEmpty(BannedPartsStr))
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
